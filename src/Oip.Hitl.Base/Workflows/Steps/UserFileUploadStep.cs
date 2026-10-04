namespace Oip.Hitl.Base.Workflows.Steps;

/// <summary>
/// File uploaded on the <c>workflow-file-upload</c> page. The file is saved to <see cref="WorkflowFileStorage"/> in
/// the step folder and attached to the step; the result describes it.
/// </summary>
public class UserFileUploadStep : UserStepDefinition<WorkflowAttachment>
{
    /// <inheritdoc />
    public override string Route => "workflow-file-upload";

    /// <summary>
    /// Allowed file extensions with the leading dot, e.g. <c>.txt</c>; empty to allow any file.
    /// </summary>
    public IReadOnlyList<string> Extensions
    {
        get;
        init => field = value.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    } = [];

    /// <summary>
    /// Maximum file size in bytes; <c>null</c> for the storage limit.
    /// </summary>
    public long? MaxFileSize { get; init; }

    /// <inheritdoc />
    public override object Data => new { Extensions, MaxFileSize };

    /// <inheritdoc />
    public override string? Validate(WorkflowAttachment result)
    {
        if (string.IsNullOrWhiteSpace(result.FileName))
            return "File is required";
        if (Extensions.Count > 0 &&
            !Extensions.Contains(Path.GetExtension(result.FileName), StringComparer.OrdinalIgnoreCase))
            return $"File must have one of the extensions: {string.Join(", ", Extensions)}";
        if (MaxFileSize is { } maxFileSize && result.Size > maxFileSize)
            return $"File must not be larger than {maxFileSize} bytes";
        return null;
    }

    /// <inheritdoc />
    public override IReadOnlyList<WorkflowAttachment> GetAttachments(WorkflowAttachment result) => [result];
}
