﻿using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sora.Database.Migrations
{
    public partial class Leaderboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "ReplayMd5",
                "Scores",
                nullable: true);

            migrationBuilder.CreateTable(
                "Leaderboard",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RankedScoreOsu = table.Column<ulong>(nullable: false),
                    RankedScoreTaiko = table.Column<ulong>(nullable: false),
                    RankedScoreCtb = table.Column<ulong>(nullable: false),
                    RankedScoreMania = table.Column<ulong>(nullable: false),
                    TotalScoreOsu = table.Column<ulong>(nullable: false),
                    TotalScoreTaiko = table.Column<ulong>(nullable: false),
                    TotalScoreCtb = table.Column<ulong>(nullable: false),
                    TotalScoreMania = table.Column<ulong>(nullable: false),
                    PlayCountOsu = table.Column<ulong>(nullable: false),
                    PlayCountTaiko = table.Column<ulong>(nullable: false),
                    PlayCountCtb = table.Column<ulong>(nullable: false),
                    PlayCountMania = table.Column<ulong>(nullable: false),
                    PerformancePointsOsu = table.Column<double>(nullable: false),
                    PerformancePointsTaiko = table.Column<double>(nullable: false),
                    PerformancePointsCtb = table.Column<double>(nullable: false),
                    PerformancePointsMania = table.Column<double>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboard", x => x.Id);
                    table.ForeignKey(
                        "FK_Leaderboard_Users_Id",
                        x => x.Id,
                        "Users",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Leaderboard");

            migrationBuilder.DropColumn(
                "ReplayMd5",
                "Scores");
        }
    }
}