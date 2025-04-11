using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddSoundcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSoundcodeSeperation",
                table: "SongScheduleSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TitleSeperation",
                table: "SongScheduleSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SongScheduleSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MaxSoundcodeSeperation", "TitleSeperation" },
                values: new object[] { 0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSoundcodeSeperation",
                table: "SongScheduleSettings");

            migrationBuilder.DropColumn(
                name: "TitleSeperation",
                table: "SongScheduleSettings");
        }
    }
}
