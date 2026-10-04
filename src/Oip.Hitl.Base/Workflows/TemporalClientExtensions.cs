using Microsoft.AspNetCore.Http;
using Oip.Base.Exceptions;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// Helpers for calling Temporal from controllers and services.
/// </summary>
public static class TemporalClientExtensions
{
    /// <summary>
    /// Runs a Temporal call and reports an unreachable server as <see cref="ApiException"/> with status 503.
    /// </summary>
    public static async Task<T> CallAsync<T>(this ITemporalClient client, Func<Task<T>> call)
    {
        try
        {
            // The client is lazy: a failed connection is reported here instead of from the first call.
            await client.Connection.ConnectAsync();
            return await call();
        }
        catch (InvalidOperationException e) when (!client.Connection.IsConnected)
        {
            throw Unavailable(e);
        }
        catch (RpcException e) when (e.Code is RpcException.StatusCode.Unavailable
                                         or RpcException.StatusCode.DeadlineExceeded)
        {
            throw Unavailable(e);
        }
    }

    private static ApiException Unavailable(Exception e) =>
        new("Workflow engine unavailable", e.Message, StatusCodes.Status503ServiceUnavailable);
}
