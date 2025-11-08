using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Oip.Rtds.Base;

public class CalculateResult
{
    public uint TagId { get; set; }
    public object Value { get; set; }
    public DateTimeOffset Time { get; set; }
    public double Error { get; set; }

    public CalculateResult()
    {
    }

    public CalculateResult(uint tagId, object value, DateTimeOffset time, double error)
    {
        TagId = tagId;
        Value = value;
        Time = time;
        Error = error;
    }
}

/// <summary>
/// Результат компиляции формулы с загруженной сборкой и методами.
/// </summary>
public class CompiledFormula : IDisposable
{
    public uint Id { get; init; }
    public string Hash { get; init; }

    public AssemblyLoadContext LoadContext { get; init; }
    public Assembly Assembly { get; init; }
    public Type FormulaType { get; init; }

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
    private readonly ConcurrentDictionary<string, CompiledFormula> _compiledByHash = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly MetadataReference[] _defaultReferences;

    public FormulaManager()
    {
        var refs = new List<MetadataReference>();
        var assembles = new List<Assembly>()
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(Math).Assembly,
            typeof(Dictionary<,>).Assembly,
            Assembly.GetExecutingAssembly()
        };

        assembles.AddRange(AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)));

        var toAddAssembles =
            assembles.Distinct();

        foreach (var a in toAddAssembles.Distinct())
            refs.Add(MetadataReference.CreateFromFile(a.Location));

        _defaultReferences = refs.ToArray();
    }

    public void UpdateFormulas(uint tagId, string valueFormula, string timeFormula, string errorFormula)
    {
        var compiled = CompileSingleFormula(tagId, valueFormula, timeFormula, errorFormula);

        _lock.EnterWriteLock();
        try
        {
            _compiled[tagId] = compiled;
        }
        finally
        {
            _lock.ExitWriteLock();
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
        var hash = ComputeSha256($"{valueFormula}{timeFormula}{errorFormula}");

        if (string.IsNullOrWhiteSpace(valueFormula.Trim()))
            valueFormula = "return value;";

        if (string.IsNullOrWhiteSpace(timeFormula.Trim()))
            timeFormula = "return time;";

        if (string.IsNullOrWhiteSpace(errorFormula.Trim()))
            errorFormula = "return 0.0;";

        _lock.EnterReadLock();
        try
        {
            if (_compiledByHash.TryGetValue(hash, out var cached))
            {
                return new CompiledFormula(id, cached.Hash, cached.LoadContext, cached.Assembly,
                    cached.FormulaType, GetMethod(cached, "ValueFormula"), GetMethod(cached, "TimeFormula"),
                    GetMethod(cached, "ErrorFormula"));
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        // Если нет — компилируем
        var formulasNamespace = "Oip.Rtds.Base.DynamicFormulas";
        var className = $"Formula_{id}";

        string source = $@"
using System;
using System.Runtime;
using System.Collections.Generic;
using Oip.Rtds.Base;

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

        ForbiddenIdentifierWalker.Validate(syntaxTree);

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

            var cf = new CompiledFormula(
                id, hash, alc, assembly, type,
                GetMethod(type, "ValueFormula"),
                GetMethod(type, "TimeFormula"),
                GetMethod(type, "ErrorFormula")
            );

            _lock.EnterWriteLock();
            try
            {
                _compiledByHash.TryAdd(hash, cf);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return cf;
        }
        catch
        {
            TryUnload(alc);
            throw;
        }
    }

    private static MethodInfo GetMethod(Type type, string name) =>
        type.GetMethod(name, BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"{name} method not found.");

    private static MethodInfo GetMethod(CompiledFormula cf, string name) =>
        cf.FormulaType.GetMethod(name, BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"{name} method not found in cached formula.");

    public async Task<CalculateResult> Evaluate(uint id, object value, object prevValue, DateTimeOffset time)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_compiled.TryGetValue(id, out var cf))
                throw new KeyNotFoundException($"Formula '{id}' not found.");

            return new CalculateResult(id,
                cf.EvaluateValue(value, time),
                cf.EvaluateTimeValue(value, time),
                cf.EvaluateErrorValue(value, time));
        }
        finally
        {
            _lock.ExitReadLock();
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
        _lock.EnterWriteLock();
        try
        {
            foreach (var kv in _compiled)
                kv.Value.Dispose();

            _compiled.Clear();
            _compiledByHash.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _lock.Dispose();
    }
}

public class ForbiddenIdentifierWalker : CSharpSyntaxWalker
{
    private readonly HashSet<string> _forbidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.IO", "File", "Directory", "Process", "System.Net", "Dns", "Socket", "HttpClient", "WebClient",
        "Thread.Sleep", "Environment.Exit", "Console.Read", "Console.ReadLine", "new Process"
    };

    private readonly List<string> _found = new();

    private IReadOnlyList<string> Found => _found;

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;
        if (_forbidden.Contains(name))
            _found.Add(name);

        base.VisitIdentifierName(node);
    }

    public override void VisitQualifiedName(QualifiedNameSyntax node)
    {
        var fullName = node.ToString();
        foreach (var forbid in _forbidden)
        {
            if (fullName.Contains(forbid, StringComparison.OrdinalIgnoreCase))
            {
                _found.Add(fullName);
                break;
            }
        }

        base.VisitQualifiedName(node);
    }

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var expr = node.ToString();
        foreach (var forbid in _forbidden)
        {
            if (expr.Contains(forbid, StringComparison.OrdinalIgnoreCase))
            {
                _found.Add(expr);
                break;
            }
        }

        base.VisitMemberAccessExpression(node);
    }

    public static void Validate(SyntaxTree syntaxTree)
    {
        var walker = new ForbiddenIdentifierWalker();
        walker.Visit(syntaxTree.GetRoot());

        if (walker.Found.Count > 0)
        {
            var list = string.Join(", ", walker.Found.Distinct(StringComparer.OrdinalIgnoreCase));
            throw new InvalidOperationException($"Class contains forbidden identifiers: {list}");
        }
    }
}