using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class PcObjectEvent_Index_Improved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_PcObjectId",
                table: "PcObjectEvents");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower",
                table: "PcObjectEvents",
                columns: new[] { "PcObjectId", "PcObjectEventTypeLower" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower",
                table: "PcObjectEvents");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectId",
                table: "PcObjectEvents",
                column: "PcObjectId");
        }
    }
}
