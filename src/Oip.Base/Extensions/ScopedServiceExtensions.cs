using Microsoft.Extensions.DependencyInjection;

namespace Oip.Base.Extensions;

/// <summary>
/// Provides extension methods for executing scoped services.
/// </summary>
public static class ScopedServiceExtensions
{
    /// <param name="factory">The service scope factory.</param>
    extension(IServiceScopeFactory factory)
    {
        /// <summary>
        /// Executes an asynchronous operation with a scoped service.
        /// </summary>
        /// <typeparam name="TService">The type of the scoped service.</typeparam>
        /// <param name="action">The asynchronous action to execute with the service.</param>
        public async Task ExecuteAsync<TService>(Func<TService, Task> action) where TService : notnull
        {
            using var scope = factory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await action(service);
        }

        /// <summary>
        /// Executes an action with a scoped service.
        /// </summary>
        /// <typeparam name="TService">The type of the scoped service.</typeparam>
        /// <param name="action">The action to execute with the service.</param>
        public void ExecuteScoped<TService>(Action<TService> action)
            where TService : notnull
        {
            using var scope = factory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            action(service);
        }

        /// <summary>
        /// Executes an asynchronous operation within a scoped service.
        /// </summary>
        /// <typeparam name="TService">The type of the scoped service.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
        /// <param name="action">The asynchronous action to execute with the service.</param>
        /// <returns>The result of the action.</returns>
        public async Task<TResult> ExecuteAsync<TService, TResult>(Func<TService, Task<TResult>> action) where TService : notnull
        {
            using var scope = factory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await action(service);
        }
    }
}