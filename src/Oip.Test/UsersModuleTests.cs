using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Controllers;
using Oip.Users.Base.Controllers.Api;
using Oip.Users.Base.Data.Entities;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Notifications;
using Oip.Users.Base.Services;

namespace Oip.Test;

public class UsersModuleTests
{
    [Test]
    public async Task GetByFilterAsync_SearchesEmailAndNamesIgnoringCase()
    {
        await using var context = await CreateUserContextAsync();
        var repository = new UserRepository(context);

        var (users, totalCount) = await repository.GetByFilterAsync("  PETROV ", null, 0, 10);

        Assert.That(users.Select(x => x.Email), Is.EqualTo(new[] { "anna@example.com", "boris@example.com" }));
        Assert.That(totalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByFilterAsync_ReturnsPageOrderedByEmailWithTotalCount()
    {
        await using var context = await CreateUserContextAsync();
        var repository = new UserRepository(context);

        var (users, totalCount) = await repository.GetByFilterAsync(null, null, 1, 1);

        Assert.That(users.Select(x => x.Email), Is.EqualTo(new[] { "boris@example.com" }));
        Assert.That(totalCount, Is.EqualTo(3));
    }

    [TestCase(true, new[] { "anna@example.com", "boris@example.com" })]
    [TestCase(false, new[] { "carl@example.com" })]
    public async Task GetByFilterAsync_FiltersByActivity(bool isActive, string[] expectedEmails)
    {
        await using var context = await CreateUserContextAsync();
        var repository = new UserRepository(context);

        var (users, totalCount) = await repository.GetByFilterAsync(null, isActive, 0, 10);

        Assert.That(users.Select(x => x.Email), Is.EqualTo(expectedEmails));
        Assert.That(totalCount, Is.EqualTo(expectedEmails.Length));
    }

    [Test]
    public async Task GetUsersSummary_CountsUsersByActivity()
    {
        await using var context = await CreateUserContextAsync();
        await using var moduleContext = CreateModuleContext();
        var controller = CreateController(context, moduleContext, new ControlledSyncCoordinator());

        var result = await controller.GetUsersSummary(CancellationToken.None);

        Assert.That(((OkObjectResult)result.Result!).Value, Is.EqualTo(new UsersSummaryDto(3, 2, 1)));
    }

    [TestCase(-1, 20)]
    [TestCase(0, 0)]
    [TestCase(0, 1001)]
    public async Task GetUsersByFilter_RejectsInvalidPage(int skip, int take)
    {
        await using var context = await CreateUserContextAsync();
        await using var moduleContext = CreateModuleContext();
        var controller = CreateController(context, moduleContext, new ControlledSyncCoordinator());

        var exception = Assert.ThrowsAsync<ApiException>(() => controller.GetUsersByFilter(
            new GetUsersByFilterRequest { Skip = skip, Take = take }, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task GetUsersByFilter_ReturnsUsersWithoutPrivateFields()
    {
        await using var context = await CreateUserContextAsync();
        await using var moduleContext = CreateModuleContext();
        var controller = CreateController(context, moduleContext, new ControlledSyncCoordinator());

        var result = await controller.GetUsersByFilter(
            new GetUsersByFilterRequest { Skip = 0, Take = 20, Search = "carl" }, CancellationToken.None);

        var page = (UsersPageDto)((OkObjectResult)result.Result!).Value!;
        Assert.That(page.TotalCount, Is.EqualTo(1));
        Assert.That(page.Users.Single(), Is.EqualTo(new UserListItemDto(3, "carl@example.com", "Carl", "Smith",
            false, page.Users.Single().LastSyncedAt)));
    }

    [Test]
    public async Task SyncAllUsers_ReturnsConflictWhileSynchronizationIsRunning()
    {
        await using var context = await CreateUserContextAsync();
        await using var moduleContext = CreateModuleContext();
        var coordinator = new ControlledSyncCoordinator();
        var controller = CreateController(context, moduleContext, coordinator);

        var started = controller.SyncAllUsers();
        var exception = Assert.Throws<ApiException>(() => controller.SyncAllUsers());
        coordinator.Complete(new SyncUsersCompleteNotify(0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        Assert.That(((AcceptedResult)started.Result!).StatusCode, Is.EqualTo(StatusCodes.Status202Accepted));
        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task GetModuleRights_DeclaresReadAndEdit()
    {
        await using var context = await CreateUserContextAsync();
        await using var moduleContext = CreateModuleContext();
        var controller = CreateController(context, moduleContext, new ControlledSyncCoordinator());

        Assert.That(controller.GetModuleRights().Select(x => x.Code),
            Is.EqualTo(new[] { SecurityConstants.Read, SecurityConstants.Edit }));
    }

    [Test]
    public async Task Coordinator_KeepsResultOfFinishedSynchronization()
    {
        var coordinator = new ControlledSyncCoordinator();
        var start = DateTimeOffset.UtcNow;

        Assert.That(coordinator.TryStart(out var running), Is.True);
        Assert.That(running.IsRunning, Is.True);
        Assert.That(coordinator.TryStart(out _), Is.False);

        coordinator.Complete(new SyncUsersCompleteNotify(42, start, start.AddSeconds(5)));
        var finished = await coordinator.WaitUntilFinishedAsync();

        Assert.That(finished, Is.EqualTo(new UsersSyncStatus
        {
            StartedAt = start,
            FinishedAt = start.AddSeconds(5),
            SyncedCount = 42
        }));
        Assert.That(coordinator.TryStart(out _), Is.True);
    }

    [Test]
    public async Task Coordinator_ReportsFailedSynchronization()
    {
        var coordinator = new ControlledSyncCoordinator();

        coordinator.TryStart(out _);
        coordinator.Fail(new InvalidOperationException("Keycloak is unavailable"));
        var finished = await coordinator.WaitUntilFinishedAsync();

        Assert.That(finished.IsRunning, Is.False);
        Assert.That(finished.Failed, Is.True);
        Assert.That(finished.SyncedCount, Is.Null);
    }

    private static UsersModuleController CreateController(UserContext context, OipModuleContext moduleContext,
        UsersSyncCoordinator coordinator)
    {
        return new UsersModuleController(new UserRepository(context), coordinator,
            new ModuleRepository(moduleContext));
    }

    private static async Task<UserContext> CreateUserContextAsync()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new UserContext(options);
        context.Users.AddRange(
            new UserEntity
            {
                UserId = 1, Email = "boris@example.com", FirstName = "Boris", LastName = "Petrov", Settings = ""
            },
            new UserEntity
            {
                UserId = 2, Email = "anna@example.com", FirstName = "Anna", LastName = "Petrova", Settings = ""
            },
            new UserEntity
            {
                UserId = 3, Email = "carl@example.com", FirstName = "Carl", LastName = "Smith", IsActive = false,
                Settings = "{\"secret\":true}"
            });
        await context.SaveChangesAsync();
        return context;
    }

    private static OipModuleContext CreateModuleContext()
    {
        var options = new DbContextOptionsBuilder<OipModuleContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OipModuleContext(options);
    }

    /// <summary>
    /// Coordinator whose synchronization finishes when the test says so.
    /// </summary>
    private sealed class ControlledSyncCoordinator() : UsersSyncCoordinator(
        new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
        NullLogger<UsersSyncCoordinator>.Instance)
    {
        private readonly TaskCompletionSource<SyncUsersCompleteNotify> _result =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Complete(SyncUsersCompleteNotify result) => _result.SetResult(result);

        public void Fail(Exception exception) => _result.SetException(exception);

        public async Task<UsersSyncStatus> WaitUntilFinishedAsync()
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (Status.IsRunning && DateTime.UtcNow < timeout)
                await Task.Delay(10);

            return Status;
        }

        protected override Task<SyncUsersCompleteNotify> SyncAllUsersAsync(IServiceProvider services) =>
            _result.Task;
    }
}
