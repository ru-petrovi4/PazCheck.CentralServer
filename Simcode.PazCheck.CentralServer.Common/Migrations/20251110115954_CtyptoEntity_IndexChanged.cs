using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class CtyptoEntity_IndexChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoEntities_Identifier",
                table: "CryptoEntities");

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "CryptoEntities",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoEntities_IdentifierLower",
                table: "CryptoEntities",
                column: "IdentifierLower",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoEntities_IdentifierLower",
                table: "CryptoEntities");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "CryptoEntities");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoEntities_Identifier",
                table: "CryptoEntities",
                column: "Identifier",
                unique: true);
        }
    }
}
