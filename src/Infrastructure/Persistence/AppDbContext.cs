using ClubMonitor.Domain.Clubs;
using ClubMonitor.Domain.Cups;
using ClubMonitor.Domain.Fixtures;
using ClubMonitor.Domain.Leagues;
using ClubMonitor.Domain.Members;
using ClubMonitor.Domain.UserProfiles;
using Microsoft.EntityFrameworkCore;

namespace ClubMonitor.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueEntry> LeagueEntries => Set<LeagueEntry>();
    public DbSet<Cup> Cups => Set<Cup>();
    public DbSet<CupEntry> CupEntries => Set<CupEntry>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

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
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("created_at");
            b.HasIndex(m => m.Email)
             .IsUnique()
             .HasDatabaseName("ix_members_email");
        });

        modelBuilder.Entity<Club>(b =>
        {
            b.ToTable("clubs");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("id");
            b.Property(c => c.Name)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("name");
            b.Property(c => c.CreatedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("created_at");
            b.HasIndex(c => c.Name)
             .IsUnique()
             .HasDatabaseName("ix_clubs_name");
        });

        modelBuilder.Entity<ClubMembership>(b =>
        {
            b.ToTable("club_memberships");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id)
             .HasConversion(id => id.Value, v => ClubMembershipId.From(v))
             .HasColumnName("id");
            b.Property(m => m.ClubId)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("club_id");
            b.Property(m => m.MemberId)
             .HasConversion(id => id.Value, v => MemberId.From(v))
             .HasColumnName("member_id");
            b.Property(m => m.Role)
             .HasColumnName("role");
            b.Property(m => m.JoinedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("joined_at");
            b.HasIndex(m => new { m.ClubId, m.MemberId })
             .IsUnique()
             .HasDatabaseName("ix_club_memberships_club_member");
        });

        modelBuilder.Entity<League>(b =>
        {
            b.ToTable("leagues");
            b.HasKey(l => l.Id);
            b.Property(l => l.Id)
             .HasConversion(id => id.Value, v => LeagueId.From(v))
             .HasColumnName("id");
            b.Property(l => l.Name)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("name");
            b.Property(l => l.CreatedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("created_at");
            b.HasIndex(l => l.Name)
             .IsUnique()
             .HasDatabaseName("ix_leagues_name");
        });

        modelBuilder.Entity<LeagueEntry>(b =>
        {
            b.ToTable("league_entries");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id)
             .HasConversion(id => id.Value, v => LeagueEntryId.From(v))
             .HasColumnName("id");
            b.Property(e => e.LeagueId)
             .HasConversion(id => id.Value, v => LeagueId.From(v))
             .HasColumnName("league_id");
            b.Property(e => e.ClubId)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("club_id");
            b.Property(e => e.EnteredAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("entered_at");
            b.HasIndex(e => new { e.LeagueId, e.ClubId })
             .IsUnique()
             .HasDatabaseName("ix_league_entries_league_club");
        });

        modelBuilder.Entity<Cup>(b =>
        {
            b.ToTable("cups");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id)
             .HasConversion(id => id.Value, v => CupId.From(v))
             .HasColumnName("id");
            b.Property(c => c.Name)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("name");
            b.Property(c => c.Status)
             .HasColumnName("status");
            b.Property(c => c.CreatedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("created_at");
            b.HasIndex(c => c.Name)
             .IsUnique()
             .HasDatabaseName("ix_cups_name");
        });

        modelBuilder.Entity<CupEntry>(b =>
        {
            b.ToTable("cup_entries");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id)
             .HasConversion(id => id.Value, v => CupEntryId.From(v))
             .HasColumnName("id");
            b.Property(e => e.CupId)
             .HasConversion(id => id.Value, v => CupId.From(v))
             .HasColumnName("cup_id");
            b.Property(e => e.ClubId)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("club_id");
            b.Property(e => e.EnteredAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("entered_at");
            b.HasIndex(e => new { e.CupId, e.ClubId })
             .IsUnique()
             .HasDatabaseName("ix_cup_entries_cup_club");
        });

        modelBuilder.Entity<Fixture>(b =>
        {
            b.ToTable("fixtures");
            b.HasKey(f => f.Id);
            b.Property(f => f.Id)
             .HasConversion(id => id.Value, v => FixtureId.From(v))
             .HasColumnName("id");
            b.Property(f => f.CompetitionType)
             .HasColumnName("competition_type");
            b.Property(f => f.CompetitionId)
             .HasColumnName("competition_id");
            b.Property(f => f.HomeClubId)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("home_club_id");
            b.Property(f => f.AwayClubId)
             .HasConversion(id => id.Value, v => ClubId.From(v))
             .HasColumnName("away_club_id");
            b.Property(f => f.ScheduledAt)
             .HasConversion(
                 v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                 v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null)
             .HasColumnName("scheduled_at");
            b.Property(f => f.Venue)
             .HasMaxLength(300)
             .HasColumnName("venue");
            b.Property(f => f.Status)
             .HasColumnName("status");
            b.Property(f => f.RoundNumber)
             .HasColumnName("round_number");
            b.Property(f => f.HomeScore)
             .HasColumnName("home_score");
            b.Property(f => f.AwayScore)
             .HasColumnName("away_score");
            b.Property(f => f.PlayedAt)
             .HasConversion(
                 v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                 v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null)
             .HasColumnName("played_at");
            b.HasIndex(f => new { f.CompetitionType, f.CompetitionId })
             .HasDatabaseName("ix_fixtures_competition");
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.ToTable("user_profiles");
            b.HasKey(u => u.Id);
            b.Property(u => u.Id)
             .HasConversion(id => id.Value, v => UserProfileId.From(v))
             .HasColumnName("id");
            b.Property(u => u.Username)
             .HasConversion(u => u.Value, v => Username.Create(v))
             .IsRequired()
             .HasMaxLength(50)
             .HasColumnName("username");
            b.Property(u => u.Email)
             .IsRequired()
             .HasMaxLength(256)
             .HasColumnName("email");
            b.Property(u => u.DisplayName)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("display_name");
            b.Property(u => u.Bio)
             .HasMaxLength(500)
             .HasColumnName("bio");
            b.Property(u => u.CreatedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("created_at");
            b.Property(u => u.UpdatedAt)
             .HasConversion(
                 v => v.ToUnixTimeMilliseconds(),
                 v => DateTimeOffset.FromUnixTimeMilliseconds(v))
             .HasColumnName("updated_at");
            b.HasIndex(u => u.Username)
             .IsUnique()
             .HasDatabaseName("ix_user_profiles_username");
            b.HasIndex(u => u.Email)
             .IsUnique()
             .HasDatabaseName("ix_user_profiles_email");
        });
    }
}