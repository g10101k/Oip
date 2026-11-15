using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Constants;
using Oip.Base.Security.Objects;
using Oip.Base.Security.Objects.Data;

namespace Oip.Demo.TableQueryDemo;

public class DemoCustomerTableContext(DbContextOptions<DemoCustomerTableContext> options)
    : DbContext(options), IAccessControlDbContext
{
    /// <summary>
    /// Schema of the demo tables.
    /// </summary>
    public const string SchemaName = "demo";

    /// <summary>
    /// Object type of a customer category, the root of the demo hierarchy.
    /// </summary>
    public const string CategoryObjectType = "demo.customer-category";

    /// <summary>
    /// Object type of a customer, inherits the rights of its category.
    /// </summary>
    public const string CustomerObjectType = "demo.customer";

    public DbSet<DemoCustomer> Customers => Set<DemoCustomer>();

    public DbSet<DemoOrder> Orders => Set<DemoOrder>();

    public DbSet<DemoCustomerCategory> Categories => Set<DemoCustomerCategory>();

    public DbSet<DemoCountry> Countries => Set<DemoCountry>();

    /// <inheritdoc />
    public DbSet<AclSetEntity> AclSets => Set<AclSetEntity>();

    /// <inheritdoc />
    public DbSet<AclSetEntryEntity> AclSetEntries => Set<AclSetEntryEntity>();

    /// <inheritdoc />
    public DbSet<AccessObjectEntity> AccessObjects => Set<AccessObjectEntity>();

    /// <inheritdoc />
    public DbSet<AccessObjectTypeEntity> AccessObjectTypes => Set<AccessObjectTypeEntity>();

    /// <inheritdoc />
    public DbSet<AccessRightEntity> AccessRights => Set<AccessRightEntity>();

    /// <summary>
    /// Registers the securable object types of the demo.
    /// </summary>
    public static void ConfigureAccessControl(AccessControlOptions<DemoCustomerTableContext> options)
    {
        options
            .Object<DemoCustomerCategory>(CategoryObjectType, o => o
                .Id(e => e.Id)
                .DefaultAcl(
                    new AclEntry(SubjectKeys.Everyone, [SecurityConstants.Read]),
                    new AclEntry(SubjectKeys.Owner, [SecurityConstants.Edit, SecurityConstants.Delete])))
            .Object<DemoCustomer>(CustomerObjectType, o => o
                .Id(e => e.Id)
                .Parent(CategoryObjectType, e => e.CategoryId)
                .Owner(e => e.OwnerSubject, fillOnInsert: true));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyOipAccessControl(Database, SchemaName);

        modelBuilder.Entity<DemoCustomer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.LifetimeValue).HasPrecision(18, 2);
            entity.HasIndex(x => x.AclSetId);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.CategoryId);

            entity.HasOne(x => x.Country)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.CountryId);
        });

        modelBuilder.Entity<DemoCustomerCategory>(entity => entity.HasIndex(x => x.AclSetId));

        modelBuilder.Entity<DemoOrder>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId);
        });
    }
}
