namespace Oip.Base.Data.Dtos;

/// Represents a Data Transfer Object for retrieving user information
public record GetUserDto(int UserId, string Email, byte[]? Photo);