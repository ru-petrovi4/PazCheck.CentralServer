using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class ParamsIndexesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagParams_TagId_ParamName",
                table: "TagParams");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId_ParamName",
                table: "SafetyControllerParams");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectJournalParams_PcObjectId",
                table: "PcObjectJournalParams");

            migrationBuilder.DropIndex(
                name: "IX_LegendParams_LegendId_ParamName",
                table: "LegendParams");

            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixParams_CeMatrixId_ParamName",
                table: "CeMatrixParams");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixComments_CeMatrixId_Identifier",
                table: "CeMatrixComments");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjectJournalParams_BasePcObjectId",
                table: "BasePcObjectJournalParams");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId_ParamName",
                table: "BaseActuatorParams");

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "TagParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "SafetyControllerParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "PcObjectJournalParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "ParamInfos",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "MetaParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "MetaParamArgs",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "LegendParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "JournalParamValuesCollections",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "CeMatrixParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentifierLower",
                table: "CeMatrixComments",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"Identifier\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "BasePcObjectJournalParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "ParamNameLower",
                table: "BaseActuatorParams",
                type: "text",
                nullable: false,
                computedColumnSql: "LOWER(\"ParamName\")",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagParams_TagId_ParamNameLower",
                table: "TagParams",
                columns: new[] { "TagId", "ParamNameLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId_ParamNameLower",
                table: "SafetyControllerParams",
                columns: new[] { "SafetyControllerId", "ParamNameLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectJournalParams_PcObjectId_ParamNameLower",
                table: "PcObjectJournalParams",
                columns: new[] { "PcObjectId", "ParamNameLower" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetaParams_ParamNameLower",
                table: "MetaParams",
                column: "ParamNameLower",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetaParamArgs_ParamNameLower",
                table: "MetaParamArgs",
                column: "ParamNameLower",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegendParams_LegendId_ParamNameLower",
                table: "LegendParams",
                columns: new[] { "LegendId", "ParamNameLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamNameLower",
                table: "JournalParamValuesCollections",
                columns: new[] { "PcObjectId", "ParamNameLower" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixParams_CeMatrixId_ParamNameLower",
                table: "CeMatrixParams",
                columns: new[] { "CeMatrixId", "ParamNameLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixComments_CeMatrixId_IdentifierLower",
                table: "CeMatrixComments",
                columns: new[] { "CeMatrixId", "IdentifierLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectJournalParams_BasePcObjectId_ParamNameLower",
                table: "BasePcObjectJournalParams",
                columns: new[] { "BasePcObjectId", "ParamNameLower" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId_ParamNameLower",
                table: "BaseActuatorParams",
                columns: new[] { "BaseActuatorId", "ParamNameLower" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagParams_TagId_ParamNameLower",
                table: "TagParams");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId_ParamNameLower",
                table: "SafetyControllerParams");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectJournalParams_PcObjectId_ParamNameLower",
                table: "PcObjectJournalParams");

            migrationBuilder.DropIndex(
                name: "IX_MetaParams_ParamNameLower",
                table: "MetaParams");

            migrationBuilder.DropIndex(
                name: "IX_MetaParamArgs_ParamNameLower",
                table: "MetaParamArgs");

            migrationBuilder.DropIndex(
                name: "IX_LegendParams_LegendId_ParamNameLower",
                table: "LegendParams");

            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamNameLower",
                table: "JournalParamValuesCollections");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixParams_CeMatrixId_ParamNameLower",
                table: "CeMatrixParams");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixComments_CeMatrixId_IdentifierLower",
                table: "CeMatrixComments");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjectJournalParams_BasePcObjectId_ParamNameLower",
                table: "BasePcObjectJournalParams");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId_ParamNameLower",
                table: "BaseActuatorParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "TagParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "SafetyControllerParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "PcObjectJournalParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "ParamInfos");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "MetaParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "MetaParamArgs");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "LegendParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "JournalParamValuesCollections");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "CeMatrixParams");

            migrationBuilder.DropColumn(
                name: "IdentifierLower",
                table: "CeMatrixComments");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "BasePcObjectJournalParams");

            migrationBuilder.DropColumn(
                name: "ParamNameLower",
                table: "BaseActuatorParams");

            migrationBuilder.CreateIndex(
                name: "IX_TagParams_TagId_ParamName",
                table: "TagParams",
                columns: new[] { "TagId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId_ParamName",
                table: "SafetyControllerParams",
                columns: new[] { "SafetyControllerId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectJournalParams_PcObjectId",
                table: "PcObjectJournalParams",
                column: "PcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LegendParams_LegendId_ParamName",
                table: "LegendParams",
                columns: new[] { "LegendId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections",
                columns: new[] { "PcObjectId", "ParamName" });

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixParams_CeMatrixId_ParamName",
                table: "CeMatrixParams",
                columns: new[] { "CeMatrixId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixComments_CeMatrixId_Identifier",
                table: "CeMatrixComments",
                columns: new[] { "CeMatrixId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectJournalParams_BasePcObjectId",
                table: "BasePcObjectJournalParams",
                column: "BasePcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId_ParamName",
                table: "BaseActuatorParams",
                columns: new[] { "BaseActuatorId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }
    }
}
