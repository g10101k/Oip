using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Oip.Rtds.Base;

public record CalculateResult(object Value, DateTimeOffset Time, double Error);

/// <summary>
/// Результат компиляции формулы с загруженной сборкой и методами.
/// </summary>
public class CompiledFormula : IDisposable
{
    // Идентификатор и контрольная сумма формулы
    public uint Id { get; init; }
    public string Hash { get; init; }

    // Загрузка сборки
    public AssemblyLoadContext LoadContext { get; init; }
    public Assembly Assembly { get; init; }
    public Type FormulaType { get; init; }

    // Методы выполнения формул
    private MethodInfo ValueMethod { get; init; }
    private MethodInfo TimeMethod { get; init; }
    private MethodInfo ErrorMethod { get; init; }

    public object EvaluateValue(object value, DateTimeOffset time) =>
        ValueMethod.Invoke(null, [value, time])!;

    public DateTimeOffset EvaluateTimeValue(object value, DateTimeOffset time) =>
        (DateTimeOffset)TimeMethod.Invoke(null, [value, time])!;

    public double EvaluateErrorValue(object value, DateTimeOffset time) =>
        Convert.ToDouble(ErrorMethod.Invoke(null, [value, time])!);

    public void Dispose()
    {
        // Можно расширить выгрузку сборки при необходимости
    }

    public CompiledFormula(uint id, string hash, AssemblyLoadContext alc, Assembly assembly,
        Type formulaType, MethodInfo valueMethod, MethodInfo timeMethod, MethodInfo errorMethod)
    {
        Id = id;
        Hash = hash;
        LoadContext = alc;
        Assembly = assembly;
        FormulaType = formulaType;
        ValueMethod = valueMethod;
        TimeMethod = timeMethod;
        ErrorMethod = errorMethod;
    }
}

class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName) => null;
}

public class FormulaManager : IDisposable
{
    private readonly ConcurrentDictionary<uint, CompiledFormula> _compiled = new();
    private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();

    private readonly HashSet<string> _forbiddenIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.IO", "File", "Directory", "Process", "System.Net", "Dns", "Socket", "HttpClient", "WebClient",
        "Thread.Sleep", "Environment.Exit", "Console.Read", "Console.ReadLine", "new Process"
    };

    private readonly MetadataReference[] _defaultReferences;

    public FormulaManager()
    {
        var refs = new List<MetadataReference>();

        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(Math).Assembly,
            typeof(Dictionary<,>).Assembly,
            Assembly.GetExecutingAssembly()
        };

        foreach (var a in assemblies.Distinct())
            refs.Add(MetadataReference.CreateFromFile(a.Location));

        _defaultReferences = refs.ToArray();
    }

    public void UpdateFormulas(uint tagId, string valueFormula, string timeFormula, string errorFormula)
    {
        var compiled = CompileSingleFormula(tagId, valueFormula, timeFormula, errorFormula);
        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            _compiled[tagId] = compiled;
        }
        finally
        {
            _readerWriterLockSlim.ExitWriteLock();
        }
    }

    private static string ComputeSha256(string text)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = sha.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private CompiledFormula CompileSingleFormula(uint id, string valueFormula, string timeFormula, string errorFormula)
    {
        foreach (var forbid in _forbiddenIdentifiers)
        {
            if (valueFormula.IndexOf(forbid, StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException($"Forbidden identifier detected: '{forbid}'");
        }

        var hash = ComputeSha256($"{valueFormula}{timeFormula}{errorFormula}");
        var formulasNamespace = "Oip.Rtds.Base.DynamicFormulas";
        var className = $"Formula_{id}";

        string source = $@"
using System;
using System.Collections.Generic;

namespace {formulasNamespace}
{{
    public static class {className}
    {{
        public static object ValueFormula(object value, DateTimeOffset time)
        {{
            try
            {{
                {valueFormula}
            }}
            catch(Exception ex)
            {{
                throw new InvalidOperationException($""ValueFormula '{id}' runtime error: {{ex.Message}}"", ex);
            }}
        }}

        public static DateTimeOffset TimeFormula(object value, DateTimeOffset time)
        {{
            try
            {{
                {timeFormula}
            }}
            catch(Exception ex)
            {{
                throw new InvalidOperationException($""TimeFormula '{id}' runtime error: {{ex.Message}}"", ex);
            }}
        }}

        public static double ErrorFormula(object value, DateTimeOffset time)
        {{
            try
            {{
                {errorFormula}
            }}
            catch(Exception ex)
            {{
                throw new InvalidOperationException($""ErrorFormula '{id}' runtime error: {{ex.Message}}"", ex);
            }}
        }}
    }}
}}";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: $"FormulaAssembly_{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: _defaultReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default)
        );

        using var peStream = new MemoryStream();
        var emitResult = compilation.Emit(peStream);

        if (!emitResult.Success)
        {
            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning)
                .Select(d =>
                {
                    var loc = d.Location;
                    string pos = loc.IsInSource ? $"(line {loc.GetLineSpan().StartLinePosition.Line + 1})" : "";
                    return $"{d.Severity}: {d.Id} {pos} - {d.GetMessage()}";
                });
            throw new InvalidOperationException($"Compilation failed for formula {id}:\n{string.Join("\n", errors)}");
        }

        peStream.Seek(0, SeekOrigin.Begin);
        var alc = new CollectibleAssemblyLoadContext();
        try
        {
            var assembly = alc.LoadFromStream(peStream);
            var type = assembly.GetType($"{formulasNamespace}.{className}")
                       ?? throw new InvalidOperationException(
                           $"Compiled assembly doesn't contain expected type '{formulasNamespace}.{className}'.");

            var methodValue = type.GetMethod("ValueFormula", BindingFlags.Public | BindingFlags.Static)
                              ?? throw new InvalidOperationException("ValueFormula method not found.");
            var methodTime = type.GetMethod("TimeFormula", BindingFlags.Public | BindingFlags.Static)
                             ?? throw new InvalidOperationException("TimeFormula method not found.");
            var methodError = type.GetMethod("ErrorFormula", BindingFlags.Public | BindingFlags.Static)
                              ?? throw new InvalidOperationException("ErrorFormula method not found.");

            return new CompiledFormula(id, hash, alc, assembly, type, methodValue, methodTime, methodError);
        }
        catch
        {
            TryUnload(alc);
            throw;
        }
    }

    public CalculateResult Evaluate(uint id, object value, DateTimeOffset time)
    {
        _readerWriterLockSlim.EnterReadLock();
        try
        {
            if (!_compiled.TryGetValue(id, out var cf))
                throw new KeyNotFoundException($"Formula '{id}' not found.");
            return new CalculateResult(cf.EvaluateValue(value, time),
                cf.EvaluateTimeValue(value, time),
                cf.EvaluateErrorValue(value, time));
        }
        finally
        {
            _readerWriterLockSlim.ExitReadLock();
        }
    }

    private void TryUnload(AssemblyLoadContext alc)
    {
        try
        {
            alc?.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            foreach (var kv in _compiled)
                kv.Value.Dispose();

            _compiled.Clear();
        }
        finally
        {
            _readerWriterLockSlim.ExitWriteLock();
        }

        _readerWriterLockSlim.Dispose();
    }
}