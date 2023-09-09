using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameOptions",
                columns: table => new
                {
                    CheckersOptionsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OptionsName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    WhiteStarts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaptureMandatory = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaptureBackwardsAllowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    KingCanFly = table.Column<bool>(type: "INTEGER", nullable: false),
                    PieceFilledRowsPerSide = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptions", x => x.CheckersOptionsId);
                });

            migrationBuilder.CreateTable(
                name: "CheckersGames",
                columns: table => new
                {
                    CheckersGameId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GameOverAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GameWonByPlayer = table.Column<string>(type: "TEXT", nullable: true),
                    Player1Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Player1Side = table.Column<int>(type: "INTEGER", nullable: false),
                    Player2Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Player2Side = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckersOptionsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckersGames", x => x.CheckersGameId);
                    table.ForeignKey(
                        name: "FK_CheckersGames_GameOptions_CheckersOptionsId",
                        column: x => x.CheckersOptionsId,
                        principalTable: "GameOptions",
                        principalColumn: "CheckersOptionsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameStates",
                columns: table => new
                {
                    GameStateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameStateName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OptionsCheckersOptionsId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameBoard = table.Column<string>(type: "TEXT", nullable: false),
                    WhiteTurn = table.Column<bool>(type: "INTEGER", nullable: false),
                    CheckersGameId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStates", x => x.GameStateId);
                    table.ForeignKey(
                        name: "FK_GameStates_CheckersGames_CheckersGameId",
                        column: x => x.CheckersGameId,
                        principalTable: "CheckersGames",
                        principalColumn: "CheckersGameId");
                    table.ForeignKey(
                        name: "FK_GameStates_GameOptions_OptionsCheckersOptionsId",
                        column: x => x.OptionsCheckersOptionsId,
                        principalTable: "GameOptions",
                        principalColumn: "CheckersOptionsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckersGames_CheckersOptionsId",
                table: "CheckersGames",
                column: "CheckersOptionsId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_CheckersGameId",
                table: "GameStates",
                column: "CheckersGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameStates_OptionsCheckersOptionsId",
                table: "GameStates",
                column: "OptionsCheckersOptionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameStates");

            migrationBuilder.DropTable(
                name: "CheckersGames");

            migrationBuilder.DropTable(
                name: "GameOptions");
        }
    }
}
