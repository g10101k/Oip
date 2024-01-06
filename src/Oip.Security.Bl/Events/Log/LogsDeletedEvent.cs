using System;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Log;

public class LogsDeletedEvent : AuditEvent
{
    public LogsDeletedEvent(DateTime deleteOlderThan)
    {
        DeleteOlderThan = deleteOlderThan;
    }

    public DateTime DeleteOlderThan { get; set; }
}