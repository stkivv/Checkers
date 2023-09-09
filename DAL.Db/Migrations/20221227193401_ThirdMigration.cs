using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class ThirdMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameStates_GameOptions_OptionsCheckersOptionsId",
                table: "GameStates");

            migrationBuilder.DropIndex(
                name: "IX_GameStates_OptionsCheckersOptionsId",
                table: "GameStates");

            migrationBuilder.DropColumn(
                name: "OptionsCheckersOptionsId",
                table: "GameStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OptionsCheckersOptionsId",
                table: "GameStates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_OptionsCheckersOptionsId",
                table: "GameStates",
                column: "OptionsCheckersOptionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameStates_GameOptions_OptionsCheckersOptionsId",
                table: "GameStates",
                column: "OptionsCheckersOptionsId",
                principalTable: "GameOptions",
                principalColumn: "CheckersOptionsId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
