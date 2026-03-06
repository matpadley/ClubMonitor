using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Add DbSet<T> later
}