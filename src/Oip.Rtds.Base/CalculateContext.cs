using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base;

/// <summary>
/// Represents a calculation result containing tag identifier, value, timestamp and error metric
/// </summary>
public class CalculateResult
{
    /// <summary>
    /// Gets or sets the numeric identifier of the tag for the calculation result
    /// </summary>
    public uint TagId { get; set; }

    /// <summary>
    /// Gets or sets the calculated value of the tag result
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the calculation result
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Gets or sets the error threshold used to determine whether a value change exceeds the allowed deviation
    /// </summary>
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
/// Result of formula compilation with loaded assembly and methods.
/// </summary>
public class CompiledFormula : IDisposable
{
    /// <summary>
    /// Gets or sets the unique identifier for the compiled formula.
    /// </summary>
    public uint Id { get; init; }

    /// <summary>
    /// Gets or sets the hash of the compiled formula.
    /// </summary>
    public string Hash { get; init; }

    /// <summary>
    /// Gets or sets the assembly load context for the compiled formula.
    /// </summary>
    public AssemblyLoadContext LoadContext { get; init; }

    /// <summary>
    /// Gets or sets the compiled assembly.
    /// </summary>
    public Assembly Assembly { get; init; }

    /// <summary>
    /// Gets or sets the formula type in the assembly.
    /// </summary>
    public Type FormulaType { get; init; }

    private MethodInfo ValueMethod { get; init; }
    private MethodInfo TimeMethod { get; init; }
    private MethodInfo ErrorMethod { get; init; }

    /// <summary>
    /// Evaluates the value formula with the provided parameters.
    /// </summary>
    /// <param name="value">The input value for the formula.</param>
    /// <param name="time">The timestamp for the calculation.</param>
    /// <returns>The calculated result.</returns>
    public object EvaluateValue(object value, DateTimeOffset time) =>
        ValueMethod.Invoke(null, [value, time])!;

    /// <summary>
    /// Evaluates the time formula with the provided parameters.
    /// </summary>
    /// <param name="value">The input value for the formula.</param>
    /// <param name="time">The timestamp for the calculation.</param>
    /// <returns>The calculated time result.</returns>
    public DateTimeOffset EvaluateTimeValue(object value, DateTimeOffset time) =>
        (DateTimeOffset)TimeMethod.Invoke(null, [value, time])!;

    /// <summary>
    /// Evaluates the error formula with the provided parameters.
    /// </summary>
    /// <param name="value">The input value for the formula.</param>
    /// <param name="time">The timestamp for the calculation.</param>
    /// <returns>The calculated error value.</returns>
    public double EvaluateErrorValue(object value, DateTimeOffset time) =>
        Convert.ToDouble(ErrorMethod.Invoke(null, [value, time])!);

    /// <summary>
    /// Disposes of the compiled formula resources.
    /// </summary>
    public void Dispose()
    {
        // Can be extended to unload the assembly if needed
    }

    /// <summary>
    /// Initializes a new instance of the CompiledFormula class.
    /// </summary>
    /// <param name="id">The unique identifier for the formula.</param>
    /// <param name="hash">The hash of the compiled formula.</param>
    /// <param name="alc">The assembly load context.</param>
    /// <param name="assembly">The compiled assembly.</param>
    /// <param name="formulaType">The formula type in the assembly.</param>
    /// <param name="valueMethod">The value calculation method.</param>
    /// <param name="timeMethod">The time calculation method.</param>
    /// <param name="errorMethod">The error calculation method.</param>
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

    public void UpdateFormulas(uint tagId, TagTypes tagTypes, string valueFormula, string timeFormula,
        string errorFormula)
    {
        var compiled = CompileSingleFormula(tagId, tagTypes, valueFormula, timeFormula, errorFormula);

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

    private CompiledFormula CompileSingleFormula(uint id, TagTypes tagTypes, string valueFormula, string timeFormula,
        string errorFormula)
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
        var typ = GetCsharpType(tagTypes);


        string source = $@"
using System;
using System.Runtime;
using System.Collections.Generic;
using Oip.Rtds.Base;

namespace {formulasNamespace}
{{
    public static class {className}
    {{
        public static {typ} ValueFormula({typ} value, DateTimeOffset time)
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

        public static DateTimeOffset TimeFormula({typ} value, DateTimeOffset time)
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

        public static {typ} ErrorFormula({typ} value, DateTimeOffset time)
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

    public CalculateResult Evaluate(uint id, object value, object prevValue, DateTimeOffset time)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_compiled.TryGetValue(id, out var cf))
                throw new KeyNotFoundException($"Formula '{id}' not found.");

            return new CalculateResult(id,
                cf.EvaluateValue(value, time),
                cf.EvaluateTimeValue(value, time),
                cf.EvaluateErrorValue(prevValue, time));
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


    private string GetCsharpType(TagTypes tagTypes)
    {
        switch (tagTypes)
        {
            case TagTypes.Float32:
                return "double";
            case TagTypes.Float64:
                return "double";
            case TagTypes.Int16:
                return "short";
            case TagTypes.Int32:
                return "int";
            case TagTypes.Digital:
                return "bool"; // для бинарных/булевых значений
            case TagTypes.String:
                return "string";
            case TagTypes.Blob:
                return "byte[]"; // или "string" если данные в base64
            default:
                throw new ArgumentOutOfRangeException(nameof(tagTypes), tagTypes, null);
        }
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