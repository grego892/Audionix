using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class DateTimeAndScheduled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actual",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "Estimated",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "Scheduled",
                table: "Log");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Log",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimePlayed",
                table: "Log",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeScheduled",
                table: "Log",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "TimePlayed",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "TimeScheduled",
                table: "Log");

            migrationBuilder.AddColumn<string>(
                name: "Actual",
                table: "Log",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estimated",
                table: "Log",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scheduled",
                table: "Log",
                type: "TEXT",
                nullable: true);
        }
    }
}
