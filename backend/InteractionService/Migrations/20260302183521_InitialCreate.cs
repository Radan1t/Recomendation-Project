using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InteractionService.Migrations
{
    
    public partial class InitialCreate : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractionLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ContentID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionLogs", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationSessions",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateGenerated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlgorithmType = table.Column<string>(type: "text", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationSessions", x => x.SessionID);
                });

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ContentID = table.Column<int>(type: "integer", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => new { x.UserID, x.ContentID });
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ContentID = table.Column<int>(type: "integer", nullable: false),
                    DateRated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => new { x.UserID, x.ContentID });
                });

            migrationBuilder.CreateTable(
                name: "RecommendedContents",
                columns: table => new
                {
                    ContentID = table.Column<int>(type: "integer", nullable: false),
                    SessionID = table.Column<int>(type: "integer", nullable: false),
                    RankPosition = table.Column<int>(type: "integer", nullable: false),
                    RelevanceScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedContents", x => new { x.SessionID, x.ContentID });
                    table.ForeignKey(
                        name: "FK_RecommendedContents_RecommendationSessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "RecommendationSessions",
                        principalColumn: "SessionID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractionLogs");

            migrationBuilder.DropTable(
                name: "RecommendedContents");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "UserRatings");

            migrationBuilder.DropTable(
                name: "RecommendationSessions");
        }
    }
}

