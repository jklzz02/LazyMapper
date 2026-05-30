using LazyMapper.TestFixtures.Models;
using Microsoft.EntityFrameworkCore;

namespace LazyMapper.Test.Projection;

internal sealed class TestProjectionDbContext(DbContextOptions<TestProjectionDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasKey(customer => customer.Id);

        modelBuilder.Entity<Order>().HasKey(order => order.Id);
        modelBuilder.Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany()
            .IsRequired();

        modelBuilder.Entity<Order>().OwnsOne(order => order.ShippingAddress);
        modelBuilder.Entity<Order>().OwnsOne(order => order.BillingAddress);
        modelBuilder.Entity<Order>().OwnsMany(order => order.Items, item =>
        {
            item.WithOwner().HasForeignKey("OrderId");
            item.Property<int>("Id");
            item.HasKey("Id");
        });

        modelBuilder.Entity<Order>().Ignore(order => order.Metadata);
    }
}