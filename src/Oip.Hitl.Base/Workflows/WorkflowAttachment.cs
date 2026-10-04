namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// File attached to a workflow step and kept in <see cref="WorkflowFileStorage"/>.
/// </summary>
/// <param name="StepId">Id of the step whose folder holds the file.</param>
/// <param name="FileName">File name within the step folder.</param>
/// <param name="ContentType">MIME type of the file.</param>
/// <param name="Size">File size in bytes.</param>
public record WorkflowAttachment(string StepId, string FileName, string ContentType, long Size);
