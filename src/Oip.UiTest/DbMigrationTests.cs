using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// The database migration module, read only: applying migrations from a test is too risky.
/// </summary>
[Order(15)]
internal class DbMigrationTests : BaseTest
{
    private const string FolderLabel = "#DbMigrationFolder";
    private const string ModuleLabel = "#DbMigrationModule";
    private const string DbMigrationModule = "DbMigration";

    /// <summary>
    /// A migration of the application database that every installation has applied.
    /// </summary>
    private const string KnownMigration = "FolderOnlyMenuRoot";

    private readonly By _folder = SideMenu.Folder(FolderLabel);

    [OneTimeSetUp]
    public void CreateModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
        Menu.CreateRootFolder(FolderLabel);
        Menu.CreateModuleInstance(_folder, ModuleLabel, DbMigrationModule);
    }

    [OneTimeTearDown]
    public void DeleteModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
    }

    [Test]
    public void MigrationsAreListed()
    {
        SignInAs(TestSetup.Admin);
        Menu.Open(SideMenu.ItemInFolder(FolderLabel, ModuleLabel));
        var migrations = new DbMigrationPage(Driver).WaitLoaded();

        Assert.That(migrations.GetNames(), Is.Not.Empty.And.All.Not.Empty);
        migrations.FilterBy(KnownMigration);
        Assert.Multiple(() =>
        {
            Assert.That(migrations.GetNames(), Has.Some.Contains(KnownMigration));
            Assert.That(migrations.IsApplied(KnownMigration), Is.True, "The migration is expected to be applied");
            Assert.That(migrations.IsPending(KnownMigration), Is.False, "The migration is expected not to be pending");
        });
    }
}
