using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class IndexAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId",
                table: "JournalParamValuesCollections");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections",
                columns: new[] { "PcObjectId", "ParamName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId",
                table: "JournalParamValuesCollections",
                column: "PcObjectId");
        }
    }
}
