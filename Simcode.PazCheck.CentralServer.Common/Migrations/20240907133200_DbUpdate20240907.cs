using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class DbUpdate20240907 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasHubArg",
                table: "MetaParams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HubGroup",
                table: "MetaParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HubMethod",
                table: "MetaParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MetaParamHubArgs",
                columns: table => new
                {
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    HubArg = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaParamHubArgs", x => x.ParamName);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StringJournalParamValues_JournalParamValuesCollectionId_Tim~",
                table: "StringJournalParamValues",
                columns: new[] { "JournalParamValuesCollectionId", "TimestampUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents",
                column: "BeginTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents",
                column: "EndTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Int32JournalParamValues_JournalParamValuesCollectionId_Time~",
                table: "Int32JournalParamValues",
                columns: new[] { "JournalParamValuesCollectionId", "TimestampUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FloatJournalParamValues_JournalParamValuesCollectionId_Time~",
                table: "FloatJournalParamValues",
                columns: new[] { "JournalParamValuesCollectionId", "TimestampUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaParamHubArgs");

            migrationBuilder.DropIndex(
                name: "IX_StringJournalParamValues_JournalParamValuesCollectionId_Tim~",
                table: "StringJournalParamValues");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_Int32JournalParamValues_JournalParamValuesCollectionId_Time~",
                table: "Int32JournalParamValues");

            migrationBuilder.DropIndex(
                name: "IX_FloatJournalParamValues_JournalParamValuesCollectionId_Time~",
                table: "FloatJournalParamValues");

            migrationBuilder.DropColumn(
                name: "HasHubArg",
                table: "MetaParams");

            migrationBuilder.DropColumn(
                name: "HubGroup",
                table: "MetaParams");

            migrationBuilder.DropColumn(
                name: "HubMethod",
                table: "MetaParams");
        }
    }
}
