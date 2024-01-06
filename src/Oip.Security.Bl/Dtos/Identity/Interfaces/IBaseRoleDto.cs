namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IBaseRoleDto
{
    object Id { get; }
    bool IsDefaultId();
}