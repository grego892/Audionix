using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AudioServerState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioServerStates");
        }
    }
}
