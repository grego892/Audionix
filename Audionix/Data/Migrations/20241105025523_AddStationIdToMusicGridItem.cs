using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStationIdToMusicGridItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StationId",
                table: "MusicGridItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StationId",
                table: "MusicGridItems");
        }
    }
}
