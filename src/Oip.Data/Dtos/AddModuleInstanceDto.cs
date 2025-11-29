namespace Oip.Base.Data.Dtos;

/// <summary>
/// Data Transfer Object for creating a new module instance
/// </summary>
/// <param name="ModuleId">The identifier of the module to create an instance of</param>
/// <param name="Label">The display label for the module instance</param>
/// <param name="Icon">The icon identifier for the module instance (optional)</param>
/// <param name="ParentId">The parent module instance identifier (optional)</param>
/// <param name="ViewRoles">Array of role identifiers that can view this module instance (optional)</param>
public record AddModuleInstanceDto(int ModuleId, string Label, string? Icon, int? ParentId, string[]? ViewRoles);

/// <summary>
/// Data Transfer Object for editing an existing module instance
/// </summary>
/// <param name="ModuleInstanceId">The identifier of the module instance to edit</param>
/// <param name="Label">The updated display label for the module instance</param>
/// <param name="Icon">The updated icon identifier for the module instance (optional)</param>
/// <param name="ParentId">The updated parent module instance identifier (optional)</param>
/// <param name="ViewRoles">Updated array of role identifiers that can view this module instance (optional)</param>
public record EditModuleInstanceDto(int ModuleInstanceId, string Label, string? Icon, int? ParentId, string[]? ViewRoles);