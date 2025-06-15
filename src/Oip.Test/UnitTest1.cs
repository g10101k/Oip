using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Oip.Base.Data.Contexts;

namespace Oip.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var b = new DbContextOptionsBuilder<OipModuleContext>();
        b.UseSqlServer();
        using var db = new OipModuleContext(b.Options);
        var currentAssembly = Assembly.GetExecutingAssembly();
        var oipAssembly = typeof(OipModuleContext).Assembly;
        var reportHandler = new OperationReportHandler(
            s => Console.WriteLine($"err:{s}"),
            s => Console.WriteLine($"warn:{s}"),
            s => Console.WriteLine($"info:{s}"),
            s => Console.WriteLine($"debug:{s}"));

        var scaffolder = new DesignTimeServicesBuilder(oipAssembly, currentAssembly,
                new OperationReporter(reportHandler), new string[0])
            .CreateServiceCollection(db)
            .BuildServiceProvider()
            .GetService<IMigrationsScaffolder>();
        var migration = scaffolder?
            .ScaffoldMigration("test", "MyProject");
        var code = migration?.MigrationCode;
        Console.WriteLine(code);
    }
}