using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class OptimizationForFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Int32JournalParamValues");

            migrationBuilder.DropTable(
                name: "StringJournalParamValues");

            migrationBuilder.AddColumn<float>(
                name: "CurrentValue",
                table: "JournalParamValuesCollections",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "JournalParamValuesCollections");

            migrationBuilder.CreateTable(
                name: "Int32JournalParamValues",
                columns: table => new
                {
                    JournalParamValuesCollectionId = table.Column<int>(type: "integer", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Int32JournalParamValues", x => new { x.JournalParamValuesCollectionId, x.TimestampUtc });
                    table.ForeignKey(
                        name: "FK_Int32JournalParamValues_JournalParamValuesCollections_Journ~",
                        column: x => x.JournalParamValuesCollectionId,
                        principalTable: "JournalParamValuesCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StringJournalParamValues",
                columns: table => new
                {
                    JournalParamValuesCollectionId = table.Column<int>(type: "integer", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringJournalParamValues", x => new { x.JournalParamValuesCollectionId, x.TimestampUtc });
                    table.ForeignKey(
                        name: "FK_StringJournalParamValues_JournalParamValuesCollections_Jour~",
                        column: x => x.JournalParamValuesCollectionId,
                        principalTable: "JournalParamValuesCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Int32JournalParamValues_JournalParamValuesCollectionId_Time~",
                table: "Int32JournalParamValues",
                columns: new[] { "JournalParamValuesCollectionId", "TimestampUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_StringJournalParamValues_JournalParamValuesCollectionId_Tim~",
                table: "StringJournalParamValues",
                columns: new[] { "JournalParamValuesCollectionId", "TimestampUtc" },
                descending: new[] { false, true });
        }
    }
}
