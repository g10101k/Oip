using System.Text.RegularExpressions;
using Oip.Settings.Enums;
using Oip.Settings.Models;

namespace Oip.Settings.Helpers;

/// <summary>
/// Helper for connection string
/// </summary>
public static class ConnectionStringHelper
{
    /// <summary>
    /// Convert DE connection string to <see cref="ConnectionModel" />
    /// </summary>
    /// <param name="connectionString">Connection string in DE format with <see cref="XpoProvider" /></param>
    /// <returns></returns>
    public static ConnectionModel NormalizeConnectionString(string connectionString)
    {
        var provider = XpoProvider.InMemoryDataStore;
        var regex = new Regex(@"XpoProvider\s*=(.*?);", (RegexOptions)531);
        var matches = regex.Matches(connectionString);

        if (matches.Count == 0)
        {
            return new ConnectionModel { Provider = provider, NormalizeConnectionString = connectionString };
        }

        connectionString = connectionString.Replace(matches[0].Value, string.Empty);
        provider = (XpoProvider)Enum.Parse(typeof(XpoProvider), matches[0].Groups[1].Value);

        return new ConnectionModel { Provider = provider, NormalizeConnectionString = connectionString };
    }
}