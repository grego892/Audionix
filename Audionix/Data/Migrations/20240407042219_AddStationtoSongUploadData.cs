using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStationtoSongUploadData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "AudioMetadatas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AudioMetadatas_StationId",
                table: "AudioMetadatas",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas");

            migrationBuilder.DropIndex(
                name: "IX_AudioMetadatas_StationId",
                table: "AudioMetadatas");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "AudioMetadatas");
        }
    }
}
