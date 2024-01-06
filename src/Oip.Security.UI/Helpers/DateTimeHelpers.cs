using System;

namespace Oip.Security.UI.Helpers;

public static class DateTimeHelpers
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static double GetEpochTicks(this DateTimeOffset dateTime)
    {
        return dateTime.Subtract(Epoch).TotalMilliseconds;
    }
}