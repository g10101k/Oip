using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Oip.Base.Exceptions;
using Oip.Hil.Base.Settings;

namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Keeps the files of workflow steps on disk, one folder per step:
/// <c>{Path}/WorkFlowAttachment/{workflowId}/{stepId}/{fileName}</c>.
/// </summary>
public class WorkflowFileStorage(WorkflowFileStorageSettings settings)
{
    /// <summary>
    /// Folder of the step files inside the storage root.
    /// </summary>
    public const string AttachmentFolder = "WorkFlowAttachment";

    private const string DefaultContentType = "application/octet-stream";
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly string _root = Path.GetFullPath(Path.Combine(settings.Path, AttachmentFolder));

    /// <summary>
    /// Maximum size of an uploaded file, in bytes.
    /// </summary>
    public long MaxFileSize => settings.MaxFileSize;

    /// <summary>
    /// Returns the full path of the step folder.
    /// </summary>
    /// <param name="workflowId">Workflow id.</param>
    /// <param name="stepId">Step id.</param>
    /// <param name="create">Whether to create the folder when it does not exist.</param>
    public string GetStepDirectory(string workflowId, string stepId, bool create = false)
    {
        var path = Path.Combine(_root, CheckSegment(workflowId), CheckSegment(stepId));
        if (create) Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Saves the file to the step folder, replacing a file with the same name.
    /// </summary>
    public async Task<WorkflowAttachment> SaveAsync(string workflowId, string stepId, string fileName, Stream content,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(GetStepDirectory(workflowId, stepId, create: true), CheckSegment(fileName));
        await using (var file = File.Create(path))
            await content.CopyToAsync(file, cancellationToken);
        return ToAttachment(stepId, new FileInfo(path));
    }

    /// <summary>
    /// Opens a file of the step for reading; returns <c>null</c> when it does not exist.
    /// </summary>
    public Stream? OpenRead(string workflowId, string stepId, string fileName)
    {
        var path = Path.Combine(GetStepDirectory(workflowId, stepId), CheckSegment(fileName));
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    /// <summary>
    /// Returns the files of the step folder ordered by name.
    /// </summary>
    public List<WorkflowAttachment> ListFiles(string workflowId, string stepId)
    {
        var directory = new DirectoryInfo(GetStepDirectory(workflowId, stepId));
        if (!directory.Exists) return [];

        return directory.EnumerateFiles()
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => ToAttachment(stepId, x))
            .ToList();
    }

    /// <summary>
    /// Deletes the step folder with all its files.
    /// </summary>
    public void ClearStep(string workflowId, string stepId)
    {
        var path = GetStepDirectory(workflowId, stepId);
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }

    /// <summary>
    /// Deletes a file of the step, if it exists.
    /// </summary>
    public void Delete(string workflowId, string stepId, string fileName)
    {
        File.Delete(Path.Combine(GetStepDirectory(workflowId, stepId), CheckSegment(fileName)));
    }

    /// <summary>
    /// Returns the MIME type for the file name.
    /// </summary>
    public static string GetContentType(string fileName) =>
        ContentTypeProvider.TryGetContentType(fileName, out var contentType) ? contentType : DefaultContentType;

    private static WorkflowAttachment ToAttachment(string stepId, FileInfo file) =>
        new(stepId, file.Name, GetContentType(file.Name), file.Length);

    /// <summary>
    /// Accepts a single file or folder name only, so a path segment cannot leave the storage.
    /// </summary>
    private static string CheckSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value is "." or ".." || Path.GetFileName(value) != value ||
            value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ApiException("Validation error", $"Invalid file or folder name '{value}'",
                StatusCodes.Status400BadRequest);
        return value;
    }
}
