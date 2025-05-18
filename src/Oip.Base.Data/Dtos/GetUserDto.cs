namespace Oip.Base.Data.Dtos;

/// <summary>
/// 
/// </summary>
/// <param name="UserId"></param>
/// <param name="Email"></param>
/// <param name="Photo"></param>
public record GetUserDto(int UserId, string Email, byte[]? Photo);