using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoundCodeId",
                table: "Log",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Log_SoundCodeId",
                table: "Log",
                column: "SoundCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_SoundCodes_SoundCodeId",
                table: "Log",
                column: "SoundCodeId",
                principalTable: "SoundCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Log_SoundCodes_SoundCodeId",
                table: "Log");

            migrationBuilder.DropIndex(
                name: "IX_Log_SoundCodeId",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "SoundCodeId",
                table: "Log");
        }
    }
}
