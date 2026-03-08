using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClubsLeaguesCupsFixtures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "club_memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_memberships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clubs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clubs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cup_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cup_id = table.Column<Guid>(type: "uuid", nullable: false),
                    club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entered_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cup_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fixtures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competition_type = table.Column<int>(type: "integer", nullable: false),
                    competition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    home_club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    away_club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<long>(type: "bigint", nullable: true),
                    venue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: true),
                    home_score = table.Column<int>(type: "integer", nullable: true),
                    away_score = table.Column<int>(type: "integer", nullable: true),
                    played_at = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fixtures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "league_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_id = table.Column<Guid>(type: "uuid", nullable: false),
                    club_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entered_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_league_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leagues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leagues", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_club_memberships_club_member",
                table: "club_memberships",
                columns: new[] { "club_id", "member_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clubs_name",
                table: "clubs",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cup_entries_cup_club",
                table: "cup_entries",
                columns: new[] { "cup_id", "club_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cups_name",
                table: "cups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fixtures_competition",
                table: "fixtures",
                columns: new[] { "competition_type", "competition_id" });

            migrationBuilder.CreateIndex(
                name: "ix_league_entries_league_club",
                table: "league_entries",
                columns: new[] { "league_id", "club_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_leagues_name",
                table: "leagues",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "club_memberships");

            migrationBuilder.DropTable(
                name: "clubs");

            migrationBuilder.DropTable(
                name: "cup_entries");

            migrationBuilder.DropTable(
                name: "cups");

            migrationBuilder.DropTable(
                name: "fixtures");

            migrationBuilder.DropTable(
                name: "league_entries");

            migrationBuilder.DropTable(
                name: "leagues");
        }
    }
}
