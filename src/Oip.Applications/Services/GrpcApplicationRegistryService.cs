using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Oip.Applications.Base;
using Oip.Applications.Base.Grpc;

namespace Oip.Applications.Services;

/// <summary>
/// gRPC application registry service.
/// </summary>
public class GrpcApplicationRegistryService(IApplicationRegistryService registryService)
    : Base.Grpc.GrpcApplicationRegistryService.GrpcApplicationRegistryServiceBase
{
    public override async Task<ApplicationRegistryItemResponse> RegisterApplication(
        RegisterApplicationRequest request,
        ServerCallContext context)
    {
        var result = await registryService.RegisterApplicationAsync(
            request.Application.ToDto(),
            context.CancellationToken);
        return new ApplicationRegistryItemResponse { Application = result.ToGrpc() };
    }

    public override async Task<ApplicationRegistryItemsResponse> GetApplicationRegistryItems(
        GetApplicationRegistryItemsRequest request,
        ServerCallContext context)
    {
        var applications = await registryService.GetApplicationRegistryItemsAsync(context.CancellationToken);
        var response = new ApplicationRegistryItemsResponse();
        response.Applications.AddRange(applications.Select(x => x.ToGrpc()));
        return response;
    }

    public override async Task<ApplicationRegistryItemResponse> GetApplicationRegistryItemByCode(
        GetApplicationRegistryItemByCodeRequest request,
        ServerCallContext context)
    {
        var result = await registryService.GetApplicationRegistryItemByCodeAsync(
            request.Code,
            context.CancellationToken);
        return new ApplicationRegistryItemResponse { Application = result.ToGrpc() };
    }

    public override async Task<ApplicationRegistryItemResponse> CreateApplicationRegistryItem(
        CreateApplicationRegistryItemRequest request,
        ServerCallContext context)
    {
        var result = await registryService.CreateApplicationRegistryItemAsync(
            request.Application.ToDto(),
            context.CancellationToken);
        return new ApplicationRegistryItemResponse { Application = result.ToGrpc() };
    }

    public override async Task<ApplicationRegistryItemResponse> UpdateApplicationRegistryItem(
        UpdateApplicationRegistryItemRequest request,
        ServerCallContext context)
    {
        var result = await registryService.UpdateApplicationRegistryItemAsync(
            request.Code,
            request.Application.ToDto(),
            context.CancellationToken);
        return new ApplicationRegistryItemResponse { Application = result.ToGrpc() };
    }

    public override async Task<Empty> DeleteApplicationRegistryItem(
        DeleteApplicationRegistryItemRequest request,
        ServerCallContext context)
    {
        await registryService.DeleteApplicationRegistryItemAsync(request.Code, context.CancellationToken);
        return new Empty();
    }
}
