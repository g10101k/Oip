namespace Oip.Security.Bl.Helpers;

public static class ViewHelpers
{
    public static string GetClientName(string clientId, string clientName)
    {
        return $"{clientId} ({clientName})";
    }
}