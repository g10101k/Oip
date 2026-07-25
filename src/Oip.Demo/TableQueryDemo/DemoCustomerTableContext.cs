using Microsoft.EntityFrameworkCore;

namespace Oip.Demo.TableQueryDemo;

public class DemoCustomerTableContext(DbContextOptions<DemoCustomerTableContext> options) : DbContext(options)
{
    public DbSet<DemoCustomer> Customers => Set<DemoCustomer>();

    public DbSet<DemoOrder> Orders => Set<DemoOrder>();

    public DbSet<DemoCustomerCategory> Categories => Set<DemoCustomerCategory>();

    public DbSet<DemoCountry> Countries => Set<DemoCountry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DemoCustomer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.LifetimeValue).HasPrecision(18, 2);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.CategoryId);

            entity.HasOne(x => x.Country)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.CountryId);
        });

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
