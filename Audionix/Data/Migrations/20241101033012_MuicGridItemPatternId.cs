using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class MuicGridItemPatternId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FridayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MondayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SaturdayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SundayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThursdayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TuesdayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WednesdayPatternId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "MondayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "SaturdayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "SundayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "ThursdayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "TuesdayPatternId",
                table: "MusicGridItems");

            migrationBuilder.DropColumn(
                name: "WednesdayPatternId",
                table: "MusicGridItems");
        }
    }
}
