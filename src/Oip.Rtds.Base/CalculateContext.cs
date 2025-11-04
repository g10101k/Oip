using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Oip.Rtds.Base;

public record CalculateResult(object Value, DateTimeOffset Time, double Error);

/// <summary>
/// Represents the result of a compilation process.
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    public List<string> Diagnostics { get; } = new();
    public CompiledTagFormulas TagFormulas { get; set; }
}

/// <summary>
/// Обертка над загруженной формулой для выполнения и выгрузки.
/// </summary>
public class CompiledTagFormulas(
    uint id,
    string hash,
    AssemblyLoadContext alc,
    Assembly assembly,
    Type formulaType,
    MethodInfo valueMethod,
    MethodInfo timeMethod,
    MethodInfo errorMethod) : IDisposable
{
    public uint Id { get; } = id;
    public string Hash { get; } = hash;
    public AssemblyLoadContext LoadContext { get; } = alc;
    public Assembly Assembly { get; } = assembly;
    public Type FormulaType { get; } = formulaType;
    private MethodInfo ValueMethod { get; } = valueMethod;
    private MethodInfo TimeMethod { get; } = timeMethod;
    private MethodInfo ErrorMethod { get; } = errorMethod;

    public object EvaluateValue(object value, DateTimeOffset time)
    {
        return ValueMethod.Invoke(null, [value, time])!;
    }

    public DateTimeOffset EvaluateTimeValue(object value, DateTimeOffset time)
    {
        return (DateTimeOffset)TimeMethod.Invoke(null, [value, time])!;
    }

    public double EvaluateErrorValue(object value, DateTimeOffset time)
    {
        return Convert.ToDouble(ErrorMethod.Invoke(null, [value, time])!);
    }

    public void Dispose()
    {
    }
}

class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Возвращаем null, чтобы использовать default probing (вдруг потребуется)
        return null;
    }
}

public class FormulaManager : IDisposable
{
    private readonly ConcurrentDictionary<uint, CompiledTagFormulas> _compiled = new();
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
        {
            refs.Add(MetadataReference.CreateFromFile(a.Location));
        }

        _defaultReferences = refs.ToArray();
    }

    public void UpdateFormulas(uint tagId, string valueFormula, string timeFormula, string errorFormula)
    {
        var results = CompileSingleFormula(tagId, valueFormula, timeFormula, errorFormula);
        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            _compiled[tagId] = results.TagFormulas;
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

    private CompilationResult CompileSingleFormula(uint id, string valueFormula, string timeFormula,
        string errorFormula)
    {
        var result = new CompilationResult();

        foreach (var forbid in _forbiddenIdentifiers)
        {
            if (valueFormula.IndexOf(forbid, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Success = false;
                result.Diagnostics.Add($"Forbidden identifier detected: '{forbid}'");
                return result;
            }
        }

        var hash = ComputeSha256($"{valueFormula}{timeFormula}{errorFormula}");

        
        var formulasNameSpace = "Oip.Rtds.Base.DynamicFormulas";
        var className = $"Formula_{id}";

        string source = $@"
using System;
using System.Collections.Generic;

namespace Oip.Rtds.Base.DynamicFormulas
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
            result.Success = false;
            foreach (var diag in emitResult.Diagnostics.Where(d =>
                         d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning))
            {
                var loc = diag.Location;
                string pos = "";
                if (loc.IsInSource)
                {
                    var span = loc.GetLineSpan();
                    pos = $"(line {span.StartLinePosition.Line + 1}, col {span.StartLinePosition.Character + 1})";
                }

                result.Diagnostics.Add($"{diag.Severity}: {diag.Id} {pos} - {diag.GetMessage()}");
            }

            return result;
        }

        peStream.Seek(0, SeekOrigin.Begin);

        var alc = new CollectibleAssemblyLoadContext();
        try
        {
            var assembly = alc.LoadFromStream(peStream);

            var fullTypeName = $"{formulasNameSpace}.{className}";
            var type = assembly.GetType(fullTypeName);
            if (type == null)
            {
                result.Success = false;
                result.Diagnostics.Add($"Compiled assembly doesn't contain expected type '{fullTypeName}'.");
                TryUnload(alc);
                return result;
            }

            var methodValueFormula = GetEvaluateMethod(type, result, "ValueFormula");
            var methodTimeFormula = GetEvaluateMethod(type, result, "TimeFormula");
            var methodErrorFormula = GetEvaluateMethod(type, result, "ErrorFormula");
            if (methodValueFormula is null || methodTimeFormula is null || methodErrorFormula is null)
            {
                TryUnload(alc);
                return result;
            }

            var compiledFormula = new CompiledTagFormulas(id, hash, alc, assembly, type, methodValueFormula!,
                methodTimeFormula!, methodErrorFormula!);
            result.Success = true;
            result.TagFormulas = compiledFormula;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Diagnostics.Add($"Exception during load: {ex.GetType().Name} {ex.Message}");
            TryUnload(alc);
            return result;
        }
    }

    private static MethodInfo? GetEvaluateMethod(Type type, CompilationResult result, string method)
    {
        var methodValueFormula = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);

        if (methodValueFormula == null)
        {
            result.Success = false;
            result.Diagnostics.Add($"Type '{type}' doesn't contain static method '{method}'.");
        }

        return methodValueFormula;
    }

    public CalculateResult Evaluate(uint id, object value, DateTimeOffset time)
    {
        _readerWriterLockSlim.EnterReadLock();
        try
        {
            if (!_compiled.TryGetValue(id, out var cf))
                throw new KeyNotFoundException($"Formula '{id}' not found.");
            return new CalculateResult(cf.EvaluateValue(value, time), cf.EvaluateTimeValue(value, time),
                cf.EvaluateErrorValue(value, time));
        }
        finally
        {
            _readerWriterLockSlim.ExitReadLock();
        }
    }

    private void UnloadCompiledFormula(CompiledTagFormulas old)
    {
        try
        {
            old.Dispose();
            var alc = old.LoadContext;
            alc.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
            // ignore
        }
    }

    private void TryUnload(AssemblyLoadContext alc)
    {
        try
        {
            alc.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
            // ignore
        }
    }

    public void Dispose()
    {
        _readerWriterLockSlim.EnterWriteLock();
        try
        {
            foreach (var kv in _compiled)
            {
                UnloadCompiledFormula(kv.Value);
            }

            _compiled.Clear();
        }
        finally
        {
            _readerWriterLockSlim.ExitWriteLock();
        }

        _readerWriterLockSlim.Dispose();
    }
}