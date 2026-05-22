using Oip.Applications.Base.Contracts;
using Oip.Applications.Base.Grpc;

namespace Oip.Applications.Base;

/// <summary>
/// Remote application registry implementation backed by gRPC.
/// </summary>
public class GrpcApplicationRegistryServiceClientAdapter(
    GrpcApplicationRegistryService.GrpcApplicationRegistryServiceClient client)
    : IApplicationRegistryService
{
    public async Task<ApplicationRegistryItemDto> RegisterApplicationAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        var response = await client.RegisterApplicationAsync(new RegisterApplicationRequest
        {
            Application = application.ToGrpc()
        }, cancellationToken: cancellationToken);
        return response.Application.ToDto();
    }

    public async Task<IReadOnlyList<ApplicationRegistryItemDto>> GetApplicationRegistryItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetApplicationRegistryItemsAsync(
            new GetApplicationRegistryItemsRequest(),
            cancellationToken: cancellationToken);
        return response.Applications.Select(x => x.ToDto()).ToList();
    }

    public async Task<ApplicationRegistryItemDto> GetApplicationRegistryItemByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetApplicationRegistryItemByCodeAsync(
            new GetApplicationRegistryItemByCodeRequest { Code = code },
            cancellationToken: cancellationToken);
        return response.Application.ToDto();
    }

    public async Task<ApplicationRegistryItemDto> CreateApplicationRegistryItemAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateApplicationRegistryItemAsync(new CreateApplicationRegistryItemRequest
        {
            Application = application.ToGrpc()
        }, cancellationToken: cancellationToken);
        return response.Application.ToDto();
    }

    public async Task<ApplicationRegistryItemDto> UpdateApplicationRegistryItemAsync(
        string code,
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        var response = await client.UpdateApplicationRegistryItemAsync(new UpdateApplicationRegistryItemRequest
        {
            Code = code,
            Application = application.ToGrpc()
        }, cancellationToken: cancellationToken);
        return response.Application.ToDto();
    }

    public async Task DeleteApplicationRegistryItemAsync(string code, CancellationToken cancellationToken = default)
    {
        await client.DeleteApplicationRegistryItemAsync(
            new DeleteApplicationRegistryItemRequest { Code = code },
            cancellationToken: cancellationToken);
    }
}
