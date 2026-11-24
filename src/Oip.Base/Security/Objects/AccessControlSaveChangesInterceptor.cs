using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Security.Objects.Data;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Keeps the access control in step with the securable entities: registers created entities with the rights of
/// their parent, follows a change of the parent and forgets deleted entities, in the transaction of the save.
/// </summary>
public sealed class AccessControlSaveChangesInterceptor<TContext>(
    AccessControlRegistry<TContext> registry,
    IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
    where TContext : DbContext, IAccessControlDbContext
{
    private readonly ConditionalWeakTable<DbContext, State> _states = new();

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Collect(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Collect(eventData.Context);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ProcessAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await ProcessAsync(eventData.Context, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => Abort(eventData.Context);

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Abort(eventData.Context);
        return Task.CompletedTask;
    }

    private void Collect(DbContext? context)
    {
        if (context is not TContext typed)
            return;

        var state = _states.GetOrCreateValue(typed);
        if (state.Busy)
            return;

        state.Clear();
        typed.ChangeTracker.DetectChanges();

        foreach (var entry in typed.ChangeTracker.Entries())
        {
            var definition = registry.FindByEntity(entry.Metadata.ClrType);
            if (definition is null)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (definition.FillOwnerOnInsert && definition.GetOwner(entry.Entity) is null)
                        definition.SetOwner(entry.Entity, ResolveResolver().Resolve().UserSubject);
                    state.Added.Add((entry.Entity, definition));
                    break;
                case EntityState.Modified when definition.ParentType is not null || definition.HasOwner:
                    state.Modified.Add((entry.Entity, definition));
                    break;
                case EntityState.Deleted:
                    state.Deleted.Add(new ObjectRef(definition.Type, definition.GetId(entry.Entity)));
                    break;
            }
        }

        if (state.HasWork && typed.Database.IsRelational() && typed.Database.CurrentTransaction is null)
            state.Transaction = typed.Database.BeginTransaction();
    }

    private async Task ProcessAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is not TContext typed || !_states.TryGetValue(typed, out var state) || state.Busy ||
            !state.HasWork)
            return;

        state.Busy = true;
        try
        {
            var service = new ObjectAccessService<TContext>(typed, registry, ResolveResolver());

            var pending = state.Added
                .Select(x => (x.Entity, x.Definition, Ref: new ObjectRef(x.Definition.Type, x.Definition.GetId(x.Entity)),
                    Parent: x.Definition.GetParent(x.Entity)))
                .ToList();
            while (pending.Count > 0)
            {
                var waiting = pending.Select(p => p.Ref).ToHashSet();
                var ready = pending.Where(p => p.Parent is null || !waiting.Contains(p.Parent.Value)).ToList();
                if (ready.Count == 0)
                    throw new InvalidOperationException("Securable objects reference each other as parents.");

                foreach (var item in ready)
                {
                    var owner = item.Definition.GetOwner(item.Entity);
                    var aclSetId = await service.RegisterAsync(item.Ref, item.Parent, owner, cancellationToken);
                    if (item.Entity is ISecurableEntity securable)
                        securable.AclSetId = aclSetId;
                    pending.Remove(item);
                }
            }

            foreach (var (entity, definition) in state.Modified)
                await service.SyncAsync(new ObjectRef(definition.Type, definition.GetId(entity)),
                    definition.GetParent(entity), definition.HasOwner, definition.GetOwner(entity), cancellationToken);

            foreach (var deleted in state.Deleted)
                await service.RemoveAsync(deleted, cancellationToken);

            await typed.SaveChangesAsync(cancellationToken);

            if (state.Transaction is not null)
                await state.Transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (state.Transaction is not null)
                await state.Transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            state.Transaction?.Dispose();
            state.Clear();
            state.Busy = false;
        }
    }

    private void Abort(DbContext? context)
    {
        if (context is not TContext typed || !_states.TryGetValue(typed, out var state) || state.Busy)
            return;

        state.Transaction?.Rollback();
        state.Transaction?.Dispose();
        state.Clear();
    }

    private ISubjectResolver ResolveResolver() =>
        httpContextAccessor.HttpContext?.RequestServices.GetService<ISubjectResolver>()
        ?? new ClaimsSubjectResolver(httpContextAccessor, []);

    private sealed class State
    {
        public bool Busy { get; set; }
        public List<(object Entity, SecurableObjectDefinition Definition)> Added { get; } = [];
        public List<(object Entity, SecurableObjectDefinition Definition)> Modified { get; } = [];
        public List<ObjectRef> Deleted { get; } = [];
        public IDbContextTransaction? Transaction { get; set; }
        public bool HasWork => Added.Count > 0 || Modified.Count > 0 || Deleted.Count > 0;

        public void Clear()
        {
            Added.Clear();
            Modified.Clear();
            Deleted.Clear();
            Transaction = null;
        }
    }
}
