using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class CaseInsensitiveIndicesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_Identifier",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_Identifier",
                table: "BasePcObjects");

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "Units",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "BasePcObjects",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_IdentifierLower",
                table: "Units",
                column: "IdentifierLower",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_IdentifierLower",
                table: "BasePcObjects",
                column: "IdentifierLower",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_IdentifierLower",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_IdentifierLower",
                table: "BasePcObjects");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "BasePcObjects");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Identifier",
                table: "Units",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_Identifier",
                table: "BasePcObjects",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }
    }
}
