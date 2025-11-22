using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Settings.Tests;

[TestFixture]
public class Tests
{
    /// <summary>
    /// Tests the NormalizeConnectionString method with different connection strings and expected providers.
    /// </summary>
    /// <param name="connectionString">The input connection string.</param>
    /// <param name="provider">The expected XpoProvider value.</param>
    [TestCase("XpoProvider=InMemoryDataStore;", XpoProvider.InMemoryDataStore)]
    [TestCase("XpoProvider=MSSqlServer;", XpoProvider.MSSqlServer)]
    [TestCase("XpoProvider=Postgres;", XpoProvider.Postgres)]
    [TestCase("XpoProvider=SQLite;", XpoProvider.SQLite)]
    public void NormalizeConnectionStringTest(string connectionString, XpoProvider provider)
    {
        // Normalize the connection string using the helper method.
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        // Assert that the provider in the normalized model matches the expected provider.
        Assert.That(provider, Is.EqualTo(model.Provider));
    }
}