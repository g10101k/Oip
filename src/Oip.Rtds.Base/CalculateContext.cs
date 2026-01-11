using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public object Value { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp of the calculation result
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Gets or sets the error threshold used to determine whether a value change exceeds the allowed deviation
    /// </summary>
    public double Error { get; set; }

    /// <summary>
    /// Result of a calculation operation containing tag data and metadata.
    /// </summary>
    public CalculateResult()
    {
    }

    /// <summary>
    /// Calculation result containing tag identifier, value, timestamp and error metric.
    /// </summary>
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
    public double EvaluateErrorValue(object? value, DateTimeOffset time) =>
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

/// <summary>
/// Syntax walker that traverses C# code to detect usage of forbidden identifiers
/// </summary>
public class ForbiddenIdentifierWalker : CSharpSyntaxWalker
{
    private readonly HashSet<string> _forbidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.IO", "File", "Directory", "Process", "System.Net", "Dns", "Socket", "HttpClient", "WebClient",
        "Thread.Sleep", "Environment.Exit", "Console.Read", "Console.ReadLine", "new Process"
    };

    private readonly List<string> _found = new();

    private IReadOnlyList<string> Found => _found;

    /// <summary>
    /// Visits an identifier name syntax node to check for forbidden identifiers.
    /// </summary>
    /// <param name="node">The identifier name syntax node to analyze</param>
    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;
        if (_forbidden.Contains(name))
            _found.Add(name);

        base.VisitIdentifierName(node);
    }

    /// <summary>
    /// Visits a qualified name syntax node to check for forbidden identifiers.
    /// </summary>
    /// <param name="node">The qualified name syntax node to analyze</param>
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

    /// <summary>
    /// Visits a member access expression node to check for forbidden identifiers.
    /// </summary>
    /// <param name="node">The member access expression syntax node to analyze</param>
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

    /// <summary>
    /// Validates syntax tree for forbidden identifiers that may pose security risks.
    /// </summary>
    /// <param name="syntaxTree">Abstract syntax tree to validate for forbidden identifiers</param>
    /// <exception cref="InvalidOperationException">Thrown when forbidden identifiers are detected in the syntax tree</exception>
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