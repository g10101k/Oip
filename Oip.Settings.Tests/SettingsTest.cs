using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Settings.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        // do nothing
    }

    [Test]
    [TestCase("XpoProvider=InMemoryDataStore;", XpoProvider.InMemoryDataStore)]
    [TestCase("XpoProvider=MSSqlServer;", XpoProvider.MSSqlServer)]
    [TestCase("XpoProvider=Postgres;", XpoProvider.Postgres)]
    [TestCase("XpoProvider=SQLite;", XpoProvider.SQLite)]
    public void NormalizeConnectionStringTest(string connectionString, XpoProvider provider)
    {
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        Assert.That(model.Provider, Is.EqualTo(provider));
    }
}