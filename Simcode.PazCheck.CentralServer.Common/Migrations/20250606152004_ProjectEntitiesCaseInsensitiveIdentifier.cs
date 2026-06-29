using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class ProjectEntitiesCaseInsensitiveIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_ProjectId_Identifier",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllers_ProjectId_Identifier",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_Legends_ProjectId_Identifier",
                table: "Legends");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrices_ProjectId_Identifier",
                table: "CeMatrices");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuators_ProjectId_Identifier",
                table: "BaseActuators");

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "Tags",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "SafetyControllers",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "Legends",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "CeMatrices",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "BaseActuators",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjectId_IdentifierLower",
                table: "Tags",
                columns: new[] { "ProjectId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_ProjectId_IdentifierLower",
                table: "SafetyControllers",
                columns: new[] { "ProjectId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Legends_ProjectId_IdentifierLower",
                table: "Legends",
                columns: new[] { "ProjectId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_ProjectId_IdentifierLower",
                table: "CeMatrices",
                columns: new[] { "ProjectId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_ProjectId_IdentifierLower",
                table: "BaseActuators",
                columns: new[] { "ProjectId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_ProjectId_IdentifierLower",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllers_ProjectId_IdentifierLower",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_Legends_ProjectId_IdentifierLower",
                table: "Legends");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrices_ProjectId_IdentifierLower",
                table: "CeMatrices");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuators_ProjectId_IdentifierLower",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "Legends");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "BaseActuators");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjectId_Identifier",
                table: "Tags",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_ProjectId_Identifier",
                table: "SafetyControllers",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Legends_ProjectId_Identifier",
                table: "Legends",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_ProjectId_Identifier",
                table: "CeMatrices",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_ProjectId_Identifier",
                table: "BaseActuators",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }
    }
}
