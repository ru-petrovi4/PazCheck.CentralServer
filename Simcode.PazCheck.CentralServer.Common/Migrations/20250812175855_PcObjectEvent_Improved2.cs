using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class PcObjectEvent_Improved2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower",
                table: "PcObjectEvents");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents",
                column: "BeginTimeUtc",
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents",
                column: "EndTimeUtc",
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower_BeginTimeU~",
                table: "PcObjectEvents",
                columns: new[] { "PcObjectId", "PcObjectEventTypeLower", "BeginTimeUtc" },
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower_BeginTimeU~",
                table: "PcObjectEvents");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_BeginTimeUtc",
                table: "PcObjectEvents",
                column: "BeginTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_EndTimeUtc",
                table: "PcObjectEvents",
                column: "EndTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectId_PcObjectEventTypeLower",
                table: "PcObjectEvents",
                columns: new[] { "PcObjectId", "PcObjectEventTypeLower" });
        }
    }
}
