using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Xunit;

namespace Oip.Settings.Test;

public class Tests
{
    [Theory]
    [InlineData("XpoProvider=InMemoryDataStore;", XpoProvider.InMemoryDataStore)]
    [InlineData("XpoProvider=MSSqlServer;", XpoProvider.MSSqlServer)]
    [InlineData("XpoProvider=Postgres;", XpoProvider.Postgres)]
    [InlineData("XpoProvider=SQLite;", XpoProvider.SQLite)]
    public void NormalizeConnectionStringTest(string connectionString, XpoProvider provider)
    {
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
        Assert.Equal(provider, model.Provider);
    }
}