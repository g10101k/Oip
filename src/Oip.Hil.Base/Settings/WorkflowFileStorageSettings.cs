namespace Oip.Hil.Base.Settings;

/// <summary>
/// Disk storage of the files attached to workflow steps.
/// </summary>
public class WorkflowFileStorageSettings
{
    /// <summary>
    /// Root folder of the storage; a relative path is resolved against the current directory of the application.
    /// Step files are kept in <c>{Path}/WorkFlowAttachment/{workflowId}/{stepId}</c>.
    /// </summary>
    public string Path { get; set; } = "FileStorage";

    /// <summary>
    /// Maximum size of an uploaded file, in bytes.
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
}
