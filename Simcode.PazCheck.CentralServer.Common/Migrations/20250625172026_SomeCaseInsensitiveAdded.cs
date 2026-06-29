using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class SomeCaseInsensitiveAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects");

            migrationBuilder.AddColumn<string>(
                name: "TypeLower",
                table: "ProjectVersionTypes",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Type\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "PcObjects",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeLower",
                table: "PcObjectEventTypes",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Type\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "PcObjectEventTypeLower",
                table: "PcObjectEvents",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"PcObjectEventType\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdLower",
                table: "ParamDescs",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Id\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionTypes_TypeLower",
                table: "ProjectVersionTypes",
                column: "TypeLower",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_IdentifierLower",
                table: "PcObjects",
                column: "IdentifierLower",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventTypes_TypeLower",
                table: "PcObjectEventTypes",
                column: "TypeLower",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParamDescs_IdLower",
                table: "ParamDescs",
                column: "IdLower",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectVersionTypes_TypeLower",
                table: "ProjectVersionTypes");

            migrationBuilder.DropIndex(
                name: "IX_PcObjects_IdentifierLower",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEventTypes_TypeLower",
                table: "PcObjectEventTypes");

            migrationBuilder.DropIndex(
                name: "IX_ParamDescs_IdLower",
                table: "ParamDescs");

            migrationBuilder.DropColumn(
                name: "TypeLower",
                table: "ProjectVersionTypes");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "PcObjects");

            migrationBuilder.DropColumn(
                name: "TypeLower",
                table: "PcObjectEventTypes");

            migrationBuilder.DropColumn(
                name: "PcObjectEventTypeLower",
                table: "PcObjectEvents");

            migrationBuilder.DropColumn(
                name: "IdLower",
                table: "ParamDescs");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects",
                column: "Identifier");
        }
    }
}
