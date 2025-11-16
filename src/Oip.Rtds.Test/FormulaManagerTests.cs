using Microsoft.CodeAnalysis.CSharp;
using Oip.Rtds.Base;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Test;

[TestFixture]
public class FormulaManagerTests : IDisposable
{
    private readonly FormulaManager _manager = new();

    [Test]
    public void UpdateFormulasTest()
    {
        _manager.UpdateFormulas(2, TagTypes.Float32, "return new Random().Next(4, 10) * value;",
            "return DateTimeOffset.Now;",
            "return new Random().Next(0, 10);");
        _manager.UpdateFormulas(3, TagTypes.Float32, "", "", "");

        var r1 = _manager.Evaluate(3, 4, null, DateTimeOffset.Now);
        var r2 = _manager.Evaluate(2, 4, null, DateTimeOffset.Now);

        Assert.That(r1.Value, Is.Not.Null);
        Assert.That(r2.Value, Is.Not.Null);
    }

    [Test]
    public void UpdateFormulas_ShouldCompileAndEvaluateSimpleExpressions()
    {
        // arrange
        uint id = 1;
        var valueFormula = "return value * 2;";
        var timeFormula = "return time.AddHours(1);";
        var errorFormula = "return 0.1;";

        _manager.UpdateFormulas(id, TagTypes.Float32, valueFormula, timeFormula, errorFormula);

        // act
        var now = DateTimeOffset.UtcNow;
        var result = _manager.Evaluate(id, 5.0, null, now);

        // assert
        Assert.That(result.Value, Is.EqualTo(10.0));
        Assert.That(result.Time, Is.EqualTo(now.AddHours(1)));
        Assert.That(result.Error, Is.EqualTo(0.1));
    }

    [Test]
    public void UpdateFormulas_ShouldUseDefaultFormulas_WhenEmptyStrings()
    {
        uint id = 2;
        _manager.UpdateFormulas(id, TagTypes.Float32, "", "", "");

        var now = DateTimeOffset.UtcNow;
        var inputValue = 123.45;

        var result = _manager.Evaluate(id, inputValue, null, now);

        Assert.That(result.Value, Is.EqualTo(inputValue));
        Assert.That(result.Time, Is.EqualTo(now));
        Assert.That(result.Error, Is.EqualTo(0.0));
    }

    [Test]
    public void UpdateFormulas_ShouldReuseCachedFormula_ByHash()
    {
        uint id1 = 10;
        uint id2 = 11;
        string val = "return value + 1;";
        string time = "return time;";
        string err = "return 0.0;";

        _manager.UpdateFormulas(id1, TagTypes.Float32, val, time, err);
        _manager.UpdateFormulas(id2, TagTypes.Float32, val, time, err); // тот же код — должен попасть в кэш

        var r1 = _manager.Evaluate(id1, 1, null, DateTimeOffset.Now);
        var r2 = _manager.Evaluate(id2, 1, null, DateTimeOffset.Now);

        Assert.That(r1.Value, Is.EqualTo(2.0));
        Assert.That(r2.Value, Is.EqualTo(2.0));
    }

    [Test]
    public void Evaluate_ShouldThrow_WhenFormulaNotExists()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() =>
            _manager.Evaluate(999, 1, null, DateTimeOffset.Now));
        Assert.That(ex.Message, Does.Contain("Formula '999' not found"));
    }

    [Test]
    public void CompileSingleFormula_ShouldThrow_OnCompilationError()
    {
        uint id = 5;
        string badValueFormula = "return unknownVariable + 1;"; // ошибка компиляции
        string timeFormula = "return time;";
        string errFormula = "return 0.0;";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _manager.UpdateFormulas(id, TagTypes.Float32, badValueFormula, timeFormula, errFormula));

        Assert.That(ex.Message, Does.Contain("Compilation failed"));
    }

    [Test]
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

        Assert.That(ex.Message, Does.Contain("System.IO"));
    }

    [Test]
    public void EvaluateValue_ShouldCallStaticMethodCorrectly()
    {
        // Компилируем формулу и достаём CompiledFormula напрямую
        uint id = 42;
        _manager.UpdateFormulas(id, TagTypes.Float32, "return value * 3;", "return time;", "return 0.5;");
        var now = DateTimeOffset.Now;
        var result = _manager.Evaluate(id, 2.0f, null, now);

        Assert.That(result.Value, Is.EqualTo(6.0));
    }

    [Test]
    public void EvaluateValueSinusoid()
    {
        // Компилируем формулу и достаём CompiledFormula напрямую
        uint id = 43;
        _manager.UpdateFormulas(id, TagTypes.Float32, "return OipRandom.Sinusoid(60, 100);", "return time;",
            "return 0.5;");
        var now = DateTimeOffset.Now;
        _ = _manager.Evaluate(id, 2, null, now);
    }

    public void Dispose()
    {
        _manager?.Dispose();
    }
}