using ClubMonitor.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(b =>
        {
            b.ToTable("members");

            b.HasKey(m => m.Id);

            b.Property(m => m.Id)
             .HasConversion(id => id.Value, value => MemberId.From(value))
             .HasColumnName("id");

            b.Property(m => m.Name)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("name");

            b.Property(m => m.Email)
             .HasConversion(e => e.Value, v => Email.Create(v))
             .IsRequired()
             .HasMaxLength(256)
             .HasColumnName("email");

            b.Property(m => m.CreatedAt)
             .HasColumnName("created_at");

            b.HasIndex(m => m.Email)
             .IsUnique()
             .HasDatabaseName("ix_members_email");
        });
    }
}