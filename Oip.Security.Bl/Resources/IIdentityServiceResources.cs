using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IIdentityServiceResources
{
    ResourceMessage IdentityErrorKey();
    ResourceMessage RoleClaimDoesNotExist();
    ResourceMessage RoleClaimsCreateFailed();
    ResourceMessage RoleClaimsUpdateFailed();
    ResourceMessage RoleClaimsDeleteFailed();
    ResourceMessage RoleCreateFailed();
    ResourceMessage RoleDeleteFailed();
    ResourceMessage RoleDoesNotExist();
    ResourceMessage RoleUpdateFailed();
    ResourceMessage UserClaimDoesNotExist();
    ResourceMessage UserClaimsCreateFailed();
    ResourceMessage UserClaimsUpdateFailed();
    ResourceMessage UserClaimsDeleteFailed();
    ResourceMessage UserCreateFailed();
    ResourceMessage UserDeleteFailed();
    ResourceMessage UserDoesNotExist();
    ResourceMessage UserChangePasswordFailed();
    ResourceMessage UserProviderDeleteFailed();
    ResourceMessage UserProviderDoesNotExist();
    ResourceMessage UserRoleCreateFailed();
    ResourceMessage UserRoleDeleteFailed();
    ResourceMessage UserUpdateFailed();
}