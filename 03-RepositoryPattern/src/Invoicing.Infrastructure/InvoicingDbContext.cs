using Microsoft.EntityFrameworkCore;

namespace Invoicing.Infrastructure;

public sealed class InvoicingDbContext(DbContextOptions<InvoicingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(invoice => invoice.Id);

            // Get-only properties are not mapped by convention, so every column is listed here.
            entity.Property(invoice => invoice.Id).ValueGeneratedNever();
            entity.Property(invoice => invoice.Number).HasMaxLength(32);
            entity.Property(invoice => invoice.CustomerName).HasMaxLength(200);
            entity.Property(invoice => invoice.Amount).HasPrecision(18, 2);
            entity.Property(invoice => invoice.DueDate);
            entity.Property(invoice => invoice.PaidOn);

            entity.HasIndex(invoice => invoice.Number).IsUnique();
        });
    }
}
