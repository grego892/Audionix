using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStationIdToProgramLogItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "Log",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Log",
                columns: new[] { "Id", "Actual", "Cart", "Category", "Cue", "Description", "Device", "Estimated", "From", "Length", "Name", "Passthrough", "Progress", "Scheduled", "Segue", "States", "StationId", "Status", "sID" },
                values: new object[] { 1, "10:58:04", "Default Cart", "COMMENT", "AutoStart", "This is a default log entry.", 1, "10:59:04", "SYSTEM", "00:00:30", "Default Log Entry", "None", 0.0, "10:58:04", "00:00:05", "isReady", 1, "Initialized", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Log_StationId",
                table: "Log",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_Stations_StationId",
                table: "Log",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Log_Stations_StationId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_StationId",
                table: "Log");

            migrationBuilder.DeleteData(
                table: "Log",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "Log");
        }
    }
}
