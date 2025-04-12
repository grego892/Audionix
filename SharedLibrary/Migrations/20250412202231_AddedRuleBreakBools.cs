using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedRuleBreakBools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BreakArtistSeperation",
                table: "SongScheduleSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BreakMaxEnergySeperation",
                table: "SongScheduleSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BreakMaxSoundCodeSeperation",
                table: "SongScheduleSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SongScheduleSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BreakArtistSeperation", "BreakMaxEnergySeperation", "BreakMaxSoundCodeSeperation" },
                values: new object[] { false, false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakArtistSeperation",
                table: "SongScheduleSettings");

            migrationBuilder.DropColumn(
                name: "BreakMaxEnergySeperation",
                table: "SongScheduleSettings");

            migrationBuilder.DropColumn(
                name: "BreakMaxSoundCodeSeperation",
                table: "SongScheduleSettings");
        }
    }
}
