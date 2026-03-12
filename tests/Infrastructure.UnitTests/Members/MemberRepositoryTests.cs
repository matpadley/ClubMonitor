using ClubMonitor.Domain.Members;
using ClubMonitor.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Reflection;

namespace Infrastructure.UnitTests.Members;

[TestFixture]
public class MemberRepositoryTests
{
    private DbContextOptions<AppDbContext> BuildOptions(string dbName)
    {
        var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var serviceProvider = new ServiceCollection().AddEntityFrameworkSqlite().BuildServiceProvider();
        builder.UseSqlite(connectionString).UseInternalServiceProvider(serviceProvider);
        return builder.Options;
    }

    [Test]
    public async Task AddAndFindById_PersistsMember()
    {
        var options = BuildOptions("members_test_db");
        using (var db = new AppDbContext(options))
        {
            db.Database.OpenConnection();
            db.Database.EnsureCreated();

            var member = Member.Create("Charlie", Email.Create("charlie@example.com"));

            await db.Members.AddAsync(member);
            await db.SaveChangesAsync();

            var found = await db.Members.FirstOrDefaultAsync(m => m.Id == member.Id);

            found.Should().NotBeNull();
            found!.Email.Value.Should().Be("charlie@example.com");
        }
    }
}


