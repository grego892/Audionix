using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class StationsCurrentPlaying : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioServerStates");

            migrationBuilder.AddColumn<int>(
                name: "CurrentPlaying",
                table: "Stations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextPlay",
                table: "Stations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPlaying",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "NextPlay",
                table: "Stations");

            migrationBuilder.CreateTable(
                name: "AudioServerStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentPlaying = table.Column<int>(type: "INTEGER", nullable: false),
                    NextPlay = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioServerStates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AudioServerStates",
                columns: new[] { "Id", "CurrentPlaying", "NextPlay" },
                values: new object[] { 1, 0, 0 });
        }
    }
}
