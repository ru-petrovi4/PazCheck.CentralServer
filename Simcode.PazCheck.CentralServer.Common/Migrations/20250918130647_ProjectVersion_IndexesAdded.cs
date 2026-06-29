using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class ProjectVersion_IndexesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId",
                table: "Cells");

            migrationBuilder.CreateIndex(
                name: "IX_TagParams_TagId__CreateProjectVersionNum",
                table: "TagParams",
                columns: new[] { "TagId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagParams_TagId__DeleteProjectVersionNum",
                table: "TagParams",
                columns: new[] { "TagId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagDbFileReferences_TagId__CreateProjectVersionNum",
                table: "TagDbFileReferences",
                columns: new[] { "TagId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagDbFileReferences_TagId__DeleteProjectVersionNum",
                table: "TagDbFileReferences",
                columns: new[] { "TagId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagConditions_TagId__CreateProjectVersionNum",
                table: "TagConditions",
                columns: new[] { "TagId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagConditions_TagId__DeleteProjectVersionNum",
                table: "TagConditions",
                columns: new[] { "TagId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_ProjectId__CreateProjectVersionNum",
                table: "SafetyControllers",
                columns: new[] { "ProjectId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_ProjectId__DeleteProjectVersionNum",
                table: "SafetyControllers",
                columns: new[] { "ProjectId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId__CreateProjectVer~",
                table: "SafetyControllerParams",
                columns: new[] { "SafetyControllerId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId__DeleteProjectVer~",
                table: "SafetyControllerParams",
                columns: new[] { "SafetyControllerId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerDbFileReferences_SafetyControllerId__Create~",
                table: "SafetyControllerDbFileReferences",
                columns: new[] { "SafetyControllerId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerDbFileReferences_SafetyControllerId__Delete~",
                table: "SafetyControllerDbFileReferences",
                columns: new[] { "SafetyControllerId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Rows_CeMatrixId__CreateProjectVersionNum",
                table: "Rows",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Rows_CeMatrixId__DeleteProjectVersionNum",
                table: "Rows",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Legends_ProjectId__CreateProjectVersionNum",
                table: "Legends",
                columns: new[] { "ProjectId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Legends_ProjectId__DeleteProjectVersionNum",
                table: "Legends",
                columns: new[] { "ProjectId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegendParams_LegendId__CreateProjectVersionNum",
                table: "LegendParams",
                columns: new[] { "LegendId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegendParams_LegendId__DeleteProjectVersionNum",
                table: "LegendParams",
                columns: new[] { "LegendId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegendDbFileReferences_LegendId__CreateProjectVersionNum",
                table: "LegendDbFileReferences",
                columns: new[] { "LegendId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegendDbFileReferences_LegendId__DeleteProjectVersionNum",
                table: "LegendDbFileReferences",
                columns: new[] { "LegendId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_CeMatrixId__CreateProjectVersionNum",
                table: "Columns",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_CeMatrixId__DeleteProjectVersionNum",
                table: "Columns",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixParams_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixParams",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixParams_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixParams",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixDbFileReferences_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixDbFileReferences",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixDbFileReferences_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixDbFileReferences",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixComments_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixComments",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixComments_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixComments",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_ProjectId__CreateProjectVersionNum",
                table: "CeMatrices",
                columns: new[] { "ProjectId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_ProjectId__DeleteProjectVersionNum",
                table: "CeMatrices",
                columns: new[] { "ProjectId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId__CreateProjectVersionNum",
                table: "Cells",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId__DeleteProjectVersionNum",
                table: "Cells",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_ProjectId__CreateProjectVersionNum",
                table: "BaseActuators",
                columns: new[] { "ProjectId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_ProjectId__DeleteProjectVersionNum",
                table: "BaseActuators",
                columns: new[] { "ProjectId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId__CreateProjectVersionNum",
                table: "BaseActuatorParams",
                columns: new[] { "BaseActuatorId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId__DeleteProjectVersionNum",
                table: "BaseActuatorParams",
                columns: new[] { "BaseActuatorId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorDbFileReferences_BaseActuatorId__CreateProjectV~",
                table: "BaseActuatorDbFileReferences",
                columns: new[] { "BaseActuatorId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorDbFileReferences_BaseActuatorId__DeleteProjectV~",
                table: "BaseActuatorDbFileReferences",
                columns: new[] { "BaseActuatorId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagParams_TagId__CreateProjectVersionNum",
                table: "TagParams");

            migrationBuilder.DropIndex(
                name: "IX_TagParams_TagId__DeleteProjectVersionNum",
                table: "TagParams");

            migrationBuilder.DropIndex(
                name: "IX_TagDbFileReferences_TagId__CreateProjectVersionNum",
                table: "TagDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_TagDbFileReferences_TagId__DeleteProjectVersionNum",
                table: "TagDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_TagConditions_TagId__CreateProjectVersionNum",
                table: "TagConditions");

            migrationBuilder.DropIndex(
                name: "IX_TagConditions_TagId__DeleteProjectVersionNum",
                table: "TagConditions");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllers_ProjectId__CreateProjectVersionNum",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllers_ProjectId__DeleteProjectVersionNum",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId__CreateProjectVer~",
                table: "SafetyControllerParams");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId__DeleteProjectVer~",
                table: "SafetyControllerParams");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerDbFileReferences_SafetyControllerId__Create~",
                table: "SafetyControllerDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllerDbFileReferences_SafetyControllerId__Delete~",
                table: "SafetyControllerDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_Rows_CeMatrixId__CreateProjectVersionNum",
                table: "Rows");

            migrationBuilder.DropIndex(
                name: "IX_Rows_CeMatrixId__DeleteProjectVersionNum",
                table: "Rows");

            migrationBuilder.DropIndex(
                name: "IX_Legends_ProjectId__CreateProjectVersionNum",
                table: "Legends");

            migrationBuilder.DropIndex(
                name: "IX_Legends_ProjectId__DeleteProjectVersionNum",
                table: "Legends");

            migrationBuilder.DropIndex(
                name: "IX_LegendParams_LegendId__CreateProjectVersionNum",
                table: "LegendParams");

            migrationBuilder.DropIndex(
                name: "IX_LegendParams_LegendId__DeleteProjectVersionNum",
                table: "LegendParams");

            migrationBuilder.DropIndex(
                name: "IX_LegendDbFileReferences_LegendId__CreateProjectVersionNum",
                table: "LegendDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_LegendDbFileReferences_LegendId__DeleteProjectVersionNum",
                table: "LegendDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_Columns_CeMatrixId__CreateProjectVersionNum",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_CeMatrixId__DeleteProjectVersionNum",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixParams_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixParams");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixParams_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixParams");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixDbFileReferences_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixDbFileReferences_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixComments_CeMatrixId__CreateProjectVersionNum",
                table: "CeMatrixComments");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixComments_CeMatrixId__DeleteProjectVersionNum",
                table: "CeMatrixComments");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrices_ProjectId__CreateProjectVersionNum",
                table: "CeMatrices");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrices_ProjectId__DeleteProjectVersionNum",
                table: "CeMatrices");

            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId__CreateProjectVersionNum",
                table: "Cells");

            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId__DeleteProjectVersionNum",
                table: "Cells");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuators_ProjectId__CreateProjectVersionNum",
                table: "BaseActuators");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuators_ProjectId__DeleteProjectVersionNum",
                table: "BaseActuators");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId__CreateProjectVersionNum",
                table: "BaseActuatorParams");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId__DeleteProjectVersionNum",
                table: "BaseActuatorParams");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorDbFileReferences_BaseActuatorId__CreateProjectV~",
                table: "BaseActuatorDbFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuatorDbFileReferences_BaseActuatorId__DeleteProjectV~",
                table: "BaseActuatorDbFileReferences");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId",
                table: "Cells",
                column: "CeMatrixId");
        }
    }
}
