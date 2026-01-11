using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base;

/// <summary>
/// Manages the compilation and evaluation of formulas for tag calculations
/// </summary>
public class FormulaManager : IDisposable
{
    private readonly ConcurrentDictionary<uint, CompiledFormula> _compiled = new();
    private readonly ConcurrentDictionary<string, CompiledFormula> _compiledByHash = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly MetadataReference[] _defaultReferences;

    /// <summary>
    /// Manages the compilation and evaluation of formulas for tag calculations
    /// </summary>
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

    /// <summary>
    /// Updates the compiled formulas for a tag with specified calculation expressions.
    /// </summary>
    /// <param name="tagId">Identifier of the tag to update formulas for.</param>
    /// <param name="tagTypes">Data type of the tag value.</param>
    /// <param name="valueFormula">Expression for calculating the tag value.</param>
    /// <param name="timeFormula">Expression for calculating the tag timestamp.</param>
    /// <param name="errorFormula">Expression for calculating the tag error value.</param>
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

    /// <summary>
    /// Evaluates a formula for a specific tag using current and previous values and timestamp
    /// </summary>
    /// <param name="id">Identifier of the tag formula to evaluate</param>
    /// <param name="value">Current value to use in formula evaluation</param>
    /// <param name="prevValue">Previous value to use in error calculation</param>
    /// <param name="time">Timestamp to use in time-based calculations</param>
    /// <returns>Calculation result containing computed value, time, and error</returns>
    /// <exception cref="KeyNotFoundException">Thrown when formula with specified id is not found</exception>
    public CalculateResult Evaluate(uint id, object value, object? prevValue, DateTimeOffset time)
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
            // 
        }
    }

    /// <inheritdoc />
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
                return "bool";
            case TagTypes.String:
                return "string";
            case TagTypes.Blob:
                return "byte[]";
            default:
                throw new ArgumentOutOfRangeException(nameof(tagTypes), tagTypes, null);
        }
    }
}