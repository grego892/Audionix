using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicPatternsData_MusicPatterns_MusicPatternId",
                table: "MusicPatternsData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MusicPatternsData",
                table: "MusicPatternsData");

            migrationBuilder.RenameTable(
                name: "MusicPatternsData",
                newName: "MusicPatternData");

            migrationBuilder.RenameIndex(
                name: "IX_MusicPatternsData_MusicPatternId",
                table: "MusicPatternData",
                newName: "IX_MusicPatternData_MusicPatternId");

            migrationBuilder.AlterColumn<int>(
                name: "StationId",
                table: "AudioMetadatas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicPatternData",
                table: "MusicPatternData",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_StationId",
                table: "Folders",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicPatternData_MusicPatterns_MusicPatternId",
                table: "MusicPatternData",
                column: "MusicPatternId",
                principalTable: "MusicPatterns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicPatternData_MusicPatterns_MusicPatternId",
                table: "MusicPatternData");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MusicPatternData",
                table: "MusicPatternData");

            migrationBuilder.RenameTable(
                name: "MusicPatternData",
                newName: "MusicPatternsData");

            migrationBuilder.RenameIndex(
                name: "IX_MusicPatternData_MusicPatternId",
                table: "MusicPatternsData",
                newName: "IX_MusicPatternsData_MusicPatternId");

            migrationBuilder.AlterColumn<int>(
                name: "StationId",
                table: "AudioMetadatas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicPatternsData",
                table: "MusicPatternsData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioMetadatas_Stations_StationId",
                table: "AudioMetadatas",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MusicPatternsData_MusicPatterns_MusicPatternId",
                table: "MusicPatternsData",
                column: "MusicPatternId",
                principalTable: "MusicPatterns",
                principalColumn: "Id");
        }
    }
}
