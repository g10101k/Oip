namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IBaseUserDto
{
    object Id { get; }
    bool IsDefaultId();
}