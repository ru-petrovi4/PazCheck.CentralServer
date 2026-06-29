using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class BasePcObject_Object_IndexUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjects_IdentifierLower",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_PcObjects_UnitId",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_IdentifierLower",
                table: "BasePcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_UnitId",
                table: "BasePcObjects");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_UnitId_IdentifierLower",
                table: "PcObjects",
                columns: new[] { "UnitId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_UnitId_IdentifierLower",
                table: "BasePcObjects",
                columns: new[] { "UnitId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjects_UnitId_IdentifierLower",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_UnitId_IdentifierLower",
                table: "BasePcObjects");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_IdentifierLower",
                table: "PcObjects",
                column: "IdentifierLower",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_UnitId",
                table: "PcObjects",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_IdentifierLower",
                table: "BasePcObjects",
                column: "IdentifierLower",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_UnitId",
                table: "BasePcObjects",
                column: "UnitId");
        }
    }
}
