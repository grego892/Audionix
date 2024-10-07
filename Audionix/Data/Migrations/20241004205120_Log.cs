using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Cue = table.Column<string>(type: "TEXT", nullable: true),
                    Scheduled = table.Column<string>(type: "TEXT", nullable: true),
                    Actual = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Cart = table.Column<string>(type: "TEXT", nullable: true),
                    Length = table.Column<string>(type: "TEXT", nullable: true),
                    Segue = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    From = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Passthrough = table.Column<string>(type: "TEXT", nullable: true),
                    States = table.Column<string>(type: "TEXT", nullable: true),
                    Device = table.Column<int>(type: "INTEGER", nullable: true),
                    sID = table.Column<int>(type: "INTEGER", nullable: true),
                    Estimated = table.Column<string>(type: "TEXT", nullable: true),
                    Progress = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log");
        }
    }
}
