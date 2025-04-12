using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedRuleBreakBools2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SongScheduleSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BreakArtistSeperation", "BreakMaxEnergySeperation", "BreakMaxSoundCodeSeperation" },
                values: new object[] { true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SongScheduleSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BreakArtistSeperation", "BreakMaxEnergySeperation", "BreakMaxSoundCodeSeperation" },
                values: new object[] { false, false, false });
        }
    }
}
