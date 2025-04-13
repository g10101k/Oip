namespace Oip.Data.Dtos;

/// <summary>
/// DTO for create module instance
/// </summary>
/// <param name="ModuleId"></param>
/// <param name="Label"></param>
/// <param name="Icon"></param>
/// <param name="ParentId"></param>
public record AddModuleInstanceDto(int ModuleId, string Label, string? Icon, int? ParentId, string[]? ViewRoles);

/// <summary>
/// DTO for edit module instance
/// </summary>
/// <param name="ModuleInstanceId"></param>
/// <param name="Label"></param>
/// <param name="Icon"></param>
/// <param name="ParentId"></param>
/// <param name="ViewRoles"></param>
public record EditModuleInstanceDto(int ModuleInstanceId, string Label, string? Icon, int? ParentId, string[]? ViewRoles);