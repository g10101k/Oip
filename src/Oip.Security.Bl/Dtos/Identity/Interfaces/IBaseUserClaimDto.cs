namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IBaseUserClaimDto
{
    int ClaimId { get; set; }
    object UserId { get; }
}