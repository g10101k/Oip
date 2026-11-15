#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Security.Objects;
using Oip.Base.Security.Objects.Data;

namespace Oip.Test;

public class ObjectAccessControlTests
{
    private const string FolderType = "test.folder";
    private const string DocumentType = "test.document";
    private const string Approve = "approve";
    private const string Export = "export";

    private static readonly SubjectSet Admin =
        new([SubjectKeys.Everyone, SubjectKeys.Role(SecurityConstants.AdminRole)], "admin-sub", true, true);

    private static readonly SubjectSet Engineer =
        new([SubjectKeys.Everyone, SubjectKeys.Role("engineers"), SubjectKeys.User("alice")], "alice", true, false);

    private static readonly SubjectSet Guest =
        new([SubjectKeys.Everyone, SubjectKeys.User("bob")], "bob", true, false);

    [Test]
    public async Task CreatedEntities_AreRegistered_AndInheritTheParentSet()
    {
        using var env = new TestEnvironment();
        var (folder, document) = await env.SeedFolderWithDocumentAsync();

        var context = env.Context;
        Assert.That(await context.AccessObjects.CountAsync(), Is.EqualTo(2));
        Assert.That(document.AclSetId, Is.EqualTo(folder.AclSetId));
        Assert.That(folder.AclSetId, Is.Not.Zero);
    }

    [Test]
    public async Task DefaultAcl_IsGivenToRootObjects()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();

        env.Subjects = Guest;
        Assert.That(await env.Access.HasRightAsync(new ObjectRef(FolderType, 1), SecurityConstants.Read), Is.True);
        Assert.That(await env.Access.HasRightAsync(new ObjectRef(FolderType, 1), SecurityConstants.Edit), Is.False);
    }

    [Test]
    public async Task SameRights_ShareOneSet()
    {
        using var env = new TestEnvironment();
        env.Context.Folders.AddRange(new Folder { Id = 1, Name = "a" }, new Folder { Id = 2, Name = "b" });
        await env.Context.SaveChangesAsync();
        var setsBefore = await env.Context.AclSets.CountAsync();

        AclEntry[] acl = [new(SubjectKeys.Role("engineers"), [SecurityConstants.Read, Approve])];
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false, acl);
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 2), false, acl);

        var folders = await env.Context.Folders.AsNoTracking().OrderBy(f => f.Id).ToListAsync();
        Assert.That(folders[0].AclSetId, Is.EqualTo(folders[1].AclSetId));
        Assert.That(await env.Context.AclSets.CountAsync(), Is.LessThanOrEqualTo(setsBefore + 2));
    }

    [Test]
    public async Task Bits_StayFixed_AndAreSharedAcrossTypes()
    {
        using var env = new TestEnvironment();
        var folderType = env.Registry.Get(FolderType);
        var documentType = env.Registry.Get(DocumentType);

        Assert.That(folderType.Bits[SecurityConstants.Read], Is.EqualTo(0));
        Assert.That(folderType.Bits[SecurityConstants.ManageAcl], Is.EqualTo(3));
        Assert.That(documentType.Bits[Approve], Is.EqualTo(folderType.Bits[Approve]));
        Assert.That(documentType.Bits[Export], Is.Not.EqualTo(folderType.Bits[Approve]));

        // A later release adds a right in front of the others: the stored bits do not move.
        var registry = new AccessControlRegistry<AclTestContext>(TestEnvironment.CreateOptions("archive"),
            env.Provider.GetRequiredService<IServiceScopeFactory>());
        var reloaded = registry.Get(FolderType);
        Assert.That(reloaded.Bits[Approve], Is.EqualTo(folderType.Bits[Approve]));
        Assert.That(reloaded.Bits["archive"], Is.GreaterThan(documentType.Bits[Export]));
    }

    [Test]
    public async Task Deny_WinsOverAllow_AndAdminBypasses()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
        [
            new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read, SecurityConstants.Edit]),
            new AclEntry(SubjectKeys.Role("engineers"), [], [SecurityConstants.Edit])
        ]);

        var document = new ObjectRef(DocumentType, 10);
        env.Subjects = Guest;
        Assert.That(await env.Access.HasRightAsync(document, SecurityConstants.Edit), Is.True);
        env.Subjects = Engineer;
        Assert.That(await env.Access.HasRightAsync(document, SecurityConstants.Edit), Is.False);
        Assert.That(await env.Access.HasRightAsync(document, SecurityConstants.Read), Is.True);
        env.Subjects = Admin;
        Assert.That(await env.Access.GetEffectiveRightsAsync(document),
            Is.EquivalentTo(env.Registry.Get(DocumentType).Definition.Rights.Select(r => r.Code)));
    }

    [Test]
    public async Task ExplicitRights_AreAddedToInheritedOnes_AndPropagate()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(DocumentType, 10), false,
            [new AclEntry(SubjectKeys.User("bob"), [SecurityConstants.Edit])]);

        env.Subjects = Guest;
        var document = new ObjectRef(DocumentType, 10);
        Assert.That(await env.Access.GetEffectiveRightsAsync(document),
            Is.EquivalentTo(new[] { SecurityConstants.Read, SecurityConstants.Edit }));

        // A change on the folder reaches the document, which keeps its explicit right.
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
            [new AclEntry(SubjectKeys.Everyone, [Approve])]);
        Assert.That(await env.Access.GetEffectiveRightsAsync(document),
            Is.EquivalentTo(new[] { SecurityConstants.Edit, Approve }));

        var stored = await env.Context.Documents.AsNoTracking().SingleAsync();
        var accessObject = await env.Context.AccessObjects.AsNoTracking()
            .SingleAsync(a => a.ObjectId == 10 && a.ObjectTypeId == env.Registry.Get(DocumentType).ObjectTypeId);
        Assert.That(stored.AclSetId, Is.EqualTo(accessObject.AclSetId));
    }

    [Test]
    public async Task BrokenInheritance_StopsPropagation()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(DocumentType, 10), true,
            [new AclEntry(SubjectKeys.Role("engineers"), [SecurityConstants.Read])]);
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
            [new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read, SecurityConstants.Delete])]);

        var document = new ObjectRef(DocumentType, 10);
        env.Subjects = Guest;
        Assert.That(await env.Access.HasRightAsync(document, SecurityConstants.Read), Is.False);
        env.Subjects = Engineer;
        Assert.That(await env.Access.GetEffectiveRightsAsync(document), Is.EquivalentTo(new[] { SecurityConstants.Read }));
    }

    [Test]
    public async Task ChangingTheParent_MovesTheObject()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        env.Context.Folders.Add(new Folder { Id = 2, Name = "secret" });
        await env.Context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 2), false, []);

        var document = await env.Context.Documents.SingleAsync();
        document.FolderId = 2;
        await env.Context.SaveChangesAsync();

        env.Subjects = Guest;
        Assert.That(await env.Access.HasRightAsync(new ObjectRef(DocumentType, 10), SecurityConstants.Read), Is.False);
        var folder = await env.Context.Folders.AsNoTracking().SingleAsync(f => f.Id == 2);
        Assert.That((await env.Context.Documents.AsNoTracking().SingleAsync()).AclSetId, Is.EqualTo(folder.AclSetId));
    }

    [Test]
    public async Task WhereAccessible_KeepsOnlyReadableEntities_AndHonoursTheOwner()
    {
        using var env = new TestEnvironment();
        env.Context.Folders.AddRange(new Folder { Id = 1, Name = "open" }, new Folder { Id = 2, Name = "closed" });
        env.Context.Documents.AddRange(
            new Document { Id = 10, FolderId = 1, Title = "public" },
            new Document { Id = 11, FolderId = 2, Title = "alice", Author = "alice" },
            new Document { Id = 12, FolderId = 2, Title = "bob", Author = "bob" });
        await env.Context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 2), false,
            [new AclEntry(SubjectKeys.Owner, [SecurityConstants.Read])]);

        env.Subjects = Engineer;
        var visible = await env.Access.WhereAccessible(env.Context.Documents, SecurityConstants.Read)
            .OrderBy(d => d.Id).Select(d => d.Title).ToListAsync();
        Assert.That(visible, Is.EqualTo(new[] { "public", "alice" }));

        env.Subjects = SubjectSet.Anonymous;
        Assert.That(await env.Access.WhereAccessible(env.Context.Documents, SecurityConstants.Read).CountAsync(),
            Is.Zero);

        env.Subjects = Admin;
        Assert.That(await env.Access.WhereAccessible(env.Context.Documents, SecurityConstants.Read).CountAsync(),
            Is.EqualTo(3));
    }

    [Test]
    public async Task GetRightsByIds_ReturnsTheRightsOfEachEntity()
    {
        using var env = new TestEnvironment();
        env.Context.Folders.AddRange(new Folder { Id = 1, Name = "open" }, new Folder { Id = 2, Name = "closed" });
        env.Context.Documents.AddRange(
            new Document { Id = 10, FolderId = 1, Title = "editable" },
            new Document { Id = 11, FolderId = 2, Title = "own", Author = "alice" },
            new Document { Id = 12, FolderId = 2, Title = "foreign", Author = "bob" });
        await env.Context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
            [new AclEntry(SubjectKeys.Role("engineers"), [SecurityConstants.Read, SecurityConstants.Edit])]);
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 2), false,
            [new AclEntry(SubjectKeys.Owner, [SecurityConstants.Read, SecurityConstants.Delete])]);

        env.Subjects = Engineer;
        var rights = await env.Access.GetRightsByIdsAsync<Document>([10, 11, 12, 99, 10],
            [SecurityConstants.Edit, SecurityConstants.Delete, Export]);

        Assert.That(rights.Keys, Is.EquivalentTo(new long[] { 10, 11, 12, 99 }));
        Assert.That(rights[10], Is.EqualTo(new[] { SecurityConstants.Edit }));
        Assert.That(rights[11], Is.EqualTo(new[] { SecurityConstants.Delete }));
        Assert.That(rights[12], Is.Empty);
        Assert.That(rights[99], Is.Empty);
    }

    [Test]
    public async Task Owner_IsFilledOnInsert_AndFollowsATransfer()
    {
        using var env = new TestEnvironment();
        env.Context.Folders.Add(new Folder { Id = 1, Name = "drafts" });
        await env.Context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
            [new AclEntry(SubjectKeys.Owner, [SecurityConstants.Read, SecurityConstants.Edit])]);

        env.Subjects = Engineer;
        var document = new Document { Id = 10, FolderId = 1, Title = "draft" };
        env.Context.Documents.Add(document);
        await env.Context.SaveChangesAsync();

        var reference = new ObjectRef(DocumentType, 10);
        Assert.That(document.Author, Is.EqualTo("alice"));
        Assert.That(await env.Access.HasRightAsync(reference, SecurityConstants.Edit), Is.True);

        document.Author = "bob";
        await env.Context.SaveChangesAsync();

        Assert.That(await env.Access.HasRightAsync(reference, SecurityConstants.Edit), Is.False);
        Assert.That(await env.Access.WhereAccessible(env.Context.Documents, SecurityConstants.Read).CountAsync(),
            Is.Zero);
        env.Subjects = Guest;
        Assert.That(await env.Access.HasRightAsync(reference, SecurityConstants.Edit), Is.True);
    }

    [Test]
    public async Task OwnerEntries_GrantNothing_OnATypeWithoutOwner()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        await env.Access.SetAclUncheckedAsync(new ObjectRef(FolderType, 1), false,
            [new AclEntry(SubjectKeys.Owner, [SecurityConstants.Read])]);

        env.Subjects = Engineer;
        Assert.That(await env.Access.HasRightAsync(new ObjectRef(FolderType, 1), SecurityConstants.Read), Is.False);
        Assert.That(await env.Access.WhereAccessible(env.Context.Folders, SecurityConstants.Read).CountAsync(),
            Is.Zero);
    }

    [Test]
    public void OwnerFilledOnInsert_MustBeAWritableProperty()
    {
        var options = new AccessControlOptions<AclTestContext>();
        Assert.Throws<ArgumentException>(() => options.Object<Document>(DocumentType, o => o
            .Id(d => d.Id)
            .Owner(d => d.Author + "!", fillOnInsert: true)));
    }

    [Test]
    public async Task DeletedEntities_AreForgotten()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        env.Context.Documents.Remove(await env.Context.Documents.SingleAsync());
        await env.Context.SaveChangesAsync();

        Assert.That(await env.Context.AccessObjects.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ManagingTheAcl_RequiresTheManageAclRight_AndValidatesInput()
    {
        using var env = new TestEnvironment();
        await env.SeedFolderWithDocumentAsync();
        var folder = new ObjectRef(FolderType, 1);

        env.Subjects = Engineer;
        var forbidden = Assert.ThrowsAsync<ApiException>(() => env.Access.GetAclAsync(folder));
        Assert.That(forbidden!.StatusCode, Is.EqualTo(403));

        await env.Access.SetAclUncheckedAsync(folder, false,
            [new AclEntry(SubjectKeys.Role("engineers"), [SecurityConstants.ManageAcl])]);
        var acl = await env.Access.GetAclAsync(folder);
        Assert.That(acl.Entries.Single().Allow, Is.EqualTo(new[] { SecurityConstants.ManageAcl }));

        var unknownRight = Assert.ThrowsAsync<ApiException>(() =>
            env.Access.SetAclAsync(folder, false, [new AclEntry(SubjectKeys.Everyone, ["fly"])]));
        Assert.That(unknownRight!.StatusCode, Is.EqualTo(400));

        var badSubject = Assert.ThrowsAsync<ApiException>(() =>
            env.Access.SetAclAsync(folder, false, [new AclEntry("nobody", [SecurityConstants.Read])]));
        Assert.That(badSubject!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void ParentOfAnotherContext_IsAConfigurationError()
    {
        var options = new AccessControlOptions<AclTestContext>();
        options.Object<Document>(DocumentType, o => o.Id(d => d.Id).Parent("other.folder", d => d.FolderId));
        Assert.Throws<InvalidOperationException>(() => new AccessControlRegistry<AclTestContext>(options, null!));
    }

    [Test]
    public void SubjectResolver_TakesRolesUserClientAndEveryone()
    {
        var identity = new System.Security.Claims.ClaimsIdentity(
        [
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "engineers"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, SecurityConstants.AdminRole),
            new System.Security.Claims.Claim("sub", "alice"),
            new System.Security.Claims.Claim("client_id", "reports")
        ], "test");

        var subjects = ClaimsSubjectResolver.Resolve(new System.Security.Claims.ClaimsPrincipal(identity));

        Assert.That(subjects.Keys, Is.EquivalentTo(new[]
        {
            SubjectKeys.Everyone, "role:engineers", "role:admin", "user:alice", "client:reports"
        }));
        Assert.That(subjects.IsAdmin, Is.True);
        Assert.That(subjects.UserSubject, Is.EqualTo("alice"));
    }

    [TestCase("Npgsql")]
    [TestCase("SqlServer")]
    public void WhereAccessible_TranslatesToABitwiseSubquery(string provider)
    {
        var options = new DbContextOptionsBuilder<AclTestContext>();
        if (provider == "Npgsql")
            options.UseNpgsql("Host=localhost;Database=acl");
        else
            options.UseSqlServer("Server=localhost;Database=acl");

        using var context = new AclTestContext(options.Options);
        var registry = new FakeRegistryContext(context);
        var access = new ObjectAccessService<AclTestContext>(context, registry.Registry, new FixedSubjectResolver(() => Engineer));

        var sql = access.WhereAccessible(context.Documents, SecurityConstants.Edit).ToQueryString();
        var script = context.Database.GenerateCreateScript();
        TestContext.Out.WriteLine(sql);

        Assert.That(sql, Does.Contain("AclSetEntry"));
        Assert.That(sql, Does.Contain("&"));
        Assert.That(sql, Does.Not.Contain("JOIN"));
        Assert.That(script, Does.Contain("AccessObject"));
        Assert.That(script, Does.Contain("AclSetEntry"));
    }

    /// <summary>
    /// Registry whose types are resolved without a database, for translating queries only.
    /// </summary>
    private sealed class FakeRegistryContext
    {
        public FakeRegistryContext(AclTestContext context)
        {
            var services = new ServiceCollection();
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<AclTestContext>(o => o.UseInMemoryDatabase(dbName));
            var provider = services.BuildServiceProvider();
            Registry = new AccessControlRegistry<AclTestContext>(TestEnvironment.CreateOptions(),
                provider.GetRequiredService<IServiceScopeFactory>());
            Registry.Initialize();
        }

        public AccessControlRegistry<AclTestContext> Registry { get; }
    }

    private sealed class TestEnvironment : IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly Microsoft.AspNetCore.Http.HttpContextAccessor _httpContextAccessor = new();

        public TestEnvironment()
        {
            var services = new ServiceCollection();
            var dbName = Guid.NewGuid().ToString();
            services.AddLogging();
            services.AddScoped<ISubjectResolver>(_ => new FixedSubjectResolver(() => Subjects));
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor>(_httpContextAccessor);
            services.AddDbContext<AclTestContext>((sp, o) =>
                o.UseInMemoryDatabase(dbName).UseOipAccessControl<AclTestContext>(sp));
            services.AddOipAccessControl<AclTestContext>(o => Configure(o));
            Provider = services.BuildServiceProvider();
            _scope = Provider.CreateScope();
            _httpContextAccessor.HttpContext =
                new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = _scope.ServiceProvider };
        }

        public ServiceProvider Provider { get; }

        public SubjectSet Subjects { get; set; } = Admin;

        public AclTestContext Context => _scope.ServiceProvider.GetRequiredService<AclTestContext>();

        public ObjectAccessService<AclTestContext> Access =>
            _scope.ServiceProvider.GetRequiredService<ObjectAccessService<AclTestContext>>();

        public AccessControlRegistry<AclTestContext> Registry =>
            Provider.GetRequiredService<AccessControlRegistry<AclTestContext>>();

        public async Task<(Folder Folder, Document Document)> SeedFolderWithDocumentAsync()
        {
            var folder = new Folder { Id = 1, Name = "projects" };
            var document = new Document { Id = 10, FolderId = 1, Title = "plan" };
            Context.Folders.Add(folder);
            Context.Documents.Add(document);
            await Context.SaveChangesAsync();
            return (folder, document);
        }

        public static AccessControlOptions<AclTestContext> CreateOptions(params string[] extraFolderRights)
        {
            var options = new AccessControlOptions<AclTestContext>();
            Configure(options, extraFolderRights);
            return options;
        }

        private static void Configure(AccessControlOptions<AclTestContext> options, params string[] extraFolderRights)
        {
            options
                .Object<Folder>(FolderType, o => o
                    .Id(f => f.Id)
                    .Rights(extraFolderRights.Append(Approve).ToArray())
                    .DefaultAcl(new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read])))
                .Object<Document>(DocumentType, o => o
                    .Id(d => d.Id)
                    .Parent(FolderType, d => d.FolderId)
                    .Owner(d => d.Author, fillOnInsert: true)
                    .Rights(Export, Approve));
        }

        public void Dispose()
        {
            _scope.Dispose();
            Provider.Dispose();
        }
    }

    private sealed class FixedSubjectResolver(Func<SubjectSet> subjects) : ISubjectResolver
    {
        public SubjectSet Resolve() => subjects();
    }

    public sealed class Folder : ISecurableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long AclSetId { get; set; }
    }

    public sealed class Document : ISecurableEntity
    {
        public int Id { get; set; }
        public int FolderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public long AclSetId { get; set; }
        public string? Author { get; set; }
    }

    public sealed class AclTestContext(DbContextOptions<AclTestContext> options) : DbContext(options), IAccessControlDbContext
    {
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<AclSetEntity> AclSets => Set<AclSetEntity>();
        public DbSet<AclSetEntryEntity> AclSetEntries => Set<AclSetEntryEntity>();
        public DbSet<AccessObjectEntity> AccessObjects => Set<AccessObjectEntity>();
        public DbSet<AccessObjectTypeEntity> AccessObjectTypes => Set<AccessObjectTypeEntity>();
        public DbSet<AccessRightEntity> AccessRights => Set<AccessRightEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyOipAccessControl(Database, "test");
            modelBuilder.Entity<Folder>().Property(f => f.Id).ValueGeneratedNever();
            modelBuilder.Entity<Document>().Property(d => d.Id).ValueGeneratedNever();
        }
    }
}
