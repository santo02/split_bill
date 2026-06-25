using Microsoft.EntityFrameworkCore;
using SplitBill.Data.Entities;

namespace SplitBill.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<Participant> Participants { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<ItemAssignee> ItemAssignees { get; set; } = null!;
    public DbSet<Payer> Payers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Bill>()
            .HasIndex(b => b.Uuid)
            .IsUnique();

        modelBuilder.Entity<Bill>()
            .HasQueryFilter(b => !b.IsDeleted);

        modelBuilder.Entity<Participant>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Item>()
            .HasQueryFilter(i => !i.IsDeleted);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.Participants)
            .WithOne(p => p.Bill)
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.Items)
            .WithOne(i => i.Bill)
            .HasForeignKey(i => i.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bill>()
            .HasMany(b => b.Payers)
            .WithOne()                   // ← only change, removed p => p.Bill
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Item>()
            .HasMany(i => i.Assignees)
            .WithOne(a => a.Item)
            .HasForeignKey(a => a.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Participant>()
            .HasMany(p => p.AssignedItems)
            .WithOne(a => a.Participant)
            .HasForeignKey(a => a.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Participant>()
            .HasMany(p => p.Payments)
            .WithOne(py => py.Participant)
            .HasForeignKey(py => py.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}