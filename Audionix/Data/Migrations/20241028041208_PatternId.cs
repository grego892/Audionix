using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audionix.Data.Migrations
{
    /// <inheritdoc />
    public partial class PatternId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PatternCategories",
                table: "PatternCategories");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PatternCategories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatternCategories",
                table: "PatternCategories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PatternCategories_MusicPatternId",
                table: "PatternCategories",
                column: "MusicPatternId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PatternCategories",
                table: "PatternCategories");

            migrationBuilder.DropIndex(
                name: "IX_PatternCategories_MusicPatternId",
                table: "PatternCategories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PatternCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatternCategories",
                table: "PatternCategories",
                columns: new[] { "MusicPatternId", "CategoryId" });
        }
    }
}
