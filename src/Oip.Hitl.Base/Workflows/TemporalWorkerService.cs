using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Hitl.Base.Settings;
using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Worker;

namespace Oip.Hitl.Base.Workflows;

/// <summary>
/// Runs the Temporal worker for the workflows and activities in <see cref="OipWorkflowOptions"/>. When the server is
/// unreachable the worker is restarted after a delay instead of stopping the application, so the rest of the app
/// works without Temporal.
/// </summary>
public class TemporalWorkerService(
    ITemporalClient client,
    TemporalSettings settings,
    OipWorkflowOptions workflowOptions,
    IServiceScopeFactory scopeFactory,
    ILoggerFactory loggerFactory,
    ILogger<TemporalWorkerService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await client.Connection.ConnectAsync();
                using var worker = new TemporalWorker(client, CreateWorkerOptions());
                logger.LogInformation("Temporal worker started on task queue {TaskQueue} at {Address}",
                    settings.TaskQueue, settings.Address);
                await worker.ExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Temporal worker is not running, retrying in {Delay}", RetryDelay);
                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private TemporalWorkerOptions CreateWorkerOptions()
    {
        var options = new TemporalWorkerOptions(settings.TaskQueue) { LoggerFactory = loggerFactory };
        foreach (var type in workflowOptions.Workflows)
            options.AddWorkflow(type);
        foreach (var type in workflowOptions.Activities)
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.IsDefined(typeof(ActivityAttribute), true))
                options.AddActivity(ActivityDefinition.Create(method, args => InvokeScopedAsync(type, method, args)));
        }

        return options;
    }

    private async Task<object?> InvokeScopedAsync(Type type, MethodInfo method, object?[] args)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var instance = method.IsStatic ? null : scope.ServiceProvider.GetRequiredService(type);
        object? result;
        try
        {
            result = method.Invoke(instance, args);
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }

        if (result is not Task task) return result;
        await task;
        // The declared return type decides: an async Task method returns Task<VoidTaskResult> at runtime.
        return method.ReturnType.IsGenericType
            ? method.ReturnType.GetProperty(nameof(Task<object>.Result))!.GetValue(task)
            : null;
    }
}
