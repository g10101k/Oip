using Microsoft.CodeAnalysis.CSharp;
using Oip.Rtds.Base;
using Oip.Rtds.Grpc;
using Xunit;

namespace Oip.Rtds.Test;

public class FormulaManagerTests : IDisposable
{
    private readonly FormulaManager _manager = new();

    [Fact]
    public async Task UpdateFormulasTest()
    {
        _manager.UpdateFormulas(2, TagTypes.Float32, "return new Random().Next(4, 10) * value;",
            "return DateTimeOffset.Now;",
            "return new Random().Next(0, 10);");
        _manager.UpdateFormulas(3, TagTypes.Float32, "", "", "");

        var r1 = await _manager.Evaluate(3, 4, null, DateTimeOffset.Now);
        var r2 = await _manager.Evaluate(2, 4, null, DateTimeOffset.Now);

        Assert.NotNull(r1.Value);
        Assert.NotNull(r2.Value);
    }

    [Fact]
    public async Task UpdateFormulas_ShouldCompileAndEvaluateSimpleExpressions()
    {
        // arrange
        uint id = 1;
        var valueFormula = "return value * 2;";
        var timeFormula = "return time.AddHours(1);";
        var errorFormula = "return 0.1;";

        _manager.UpdateFormulas(id, TagTypes.Float32, valueFormula, timeFormula, errorFormula);

        // act
        var now = DateTimeOffset.UtcNow;
        var result = await _manager.Evaluate(id, 5.0, null, now);

        // assert
        Assert.Equal(10.0, result.Value);
        Assert.Equal(now.AddHours(1), result.Time);
        Assert.Equal(0.1, result.Error);
    }

    [Fact]
    public async Task UpdateFormulas_ShouldUseDefaultFormulas_WhenEmptyStrings()
    {
        uint id = 2;
        _manager.UpdateFormulas(id, TagTypes.Float32, "", "", "");

        var now = DateTimeOffset.UtcNow;
        var inputValue = 123.45;

        var result = await _manager.Evaluate(id, inputValue, null, now);

        Assert.Equal(inputValue, result.Value);
        Assert.Equal(now, result.Time);
        Assert.Equal(0.0, result.Error);
    }

    [Fact]
    public async Task UpdateFormulas_ShouldReuseCachedFormula_ByHash()
    {
        uint id1 = 10;
        uint id2 = 11;
        string val = "return value + 1;";
        string time = "return time;";
        string err = "return 0.0;";

        _manager.UpdateFormulas(id1, TagTypes.Float32, val, time, err);
        _manager.UpdateFormulas(id2, TagTypes.Float32, val, time, err); // тот же код — должен попасть в кэш

        var r1 = await _manager.Evaluate(id1, 1, null, DateTimeOffset.Now);
        var r2 = await _manager.Evaluate(id2, 1, null, DateTimeOffset.Now);

        Assert.Equal(2.0, r1.Value);
        Assert.Equal(2.0, r2.Value);
    }

    [Fact]
    public async Task Evaluate_ShouldThrow_WhenFormulaNotExists()
    {
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _manager.Evaluate(999, 1, null, DateTimeOffset.Now));
        Assert.Contains("Formula '999' not found", ex.Message);
    }

    [Fact]
    public void CompileSingleFormula_ShouldThrow_OnCompilationError()
    {
        uint id = 5;
        string badValueFormula = "return unknownVariable + 1;"; // ошибка компиляции
        string timeFormula = "return time;";
        string errFormula = "return 0.0;";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _manager.UpdateFormulas(id, TagTypes.Float32, badValueFormula, timeFormula, errFormula));

        Assert.Contains("Compilation failed", ex.Message);
    }

    [Fact]
    public void ForbiddenIdentifierWalker_ShouldThrow_OnForbiddenCode()
    {
        string badSource = @"
using System;
namespace Test
{
    public static class X
    {
        public static void M() { var f = System.IO.File.ReadAllText(""test.txt""); }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(badSource);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ForbiddenIdentifierWalker.Validate(tree));

        Assert.Contains("System.IO", ex.Message);
    }

    [Fact]
    public async Task EvaluateValue_ShouldCallStaticMethodCorrectly()
    {
        // Компилируем формулу и достаём CompiledFormula напрямую
        uint id = 42;
        _manager.UpdateFormulas(id, TagTypes.Float32, "return value * 3;", "return time;", "return 0.5;");
        var now = DateTimeOffset.Now;
        var result = await _manager.Evaluate(id, 2.0f, null, now);

        Assert.Equal(6.0, result.Value);
    }

    [Fact]
    public async Task EvaluateValueSinusoid()
    {
        // Компилируем формулу и достаём CompiledFormula напрямую
        uint id = 43;
        _manager.UpdateFormulas(id, TagTypes.Float32, "return OipRandom.Sinusoid(60, 100);", "return time;",
            "return 0.5;");
        var now = DateTimeOffset.Now;
        _ = await _manager.Evaluate(id, 2, null, now);
    }

    public void Dispose()
    {
        _manager?.Dispose();
    }
}