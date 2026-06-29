using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class UnversionedFieldsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators");

            migrationBuilder.DropForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices");

            migrationBuilder.DropForeignKey(
                name: "FK_CeMatrixResults_CeMatrices_CeMatrixId",
                table: "CeMatrixResults");

            migrationBuilder.DropForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_BaseActuators_BaseActuatorId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_BaseActuatorId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrixResults_CeMatrixId",
                table: "CeMatrixResults");

            migrationBuilder.DropColumn(
                name: "BaseActuatorId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagType",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "CeMatrixId",
                table: "CeMatrixResults");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "BaseActuators");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Results",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CeMatrixComments",
                table: "CeMatrixResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CeMatrixParams",
                table: "CeMatrixResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "CeMatrixResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ProjectId",
                table: "Results",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators",
                column: "BaseActuatorTypeId",
                principalTable: "BaseActuatorTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices",
                column: "CeMatrixTypeId",
                principalTable: "CeMatrixTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Projects_ProjectId",
                table: "Results",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers",
                column: "SafetyControllerTypeId",
                principalTable: "SafetyControllerTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators");

            migrationBuilder.DropForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Projects_ProjectId",
                table: "Results");

            migrationBuilder.DropForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_Results_ProjectId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Results");

            migrationBuilder.DropColumn(
                name: "CeMatrixComments",
                table: "CeMatrixResults");

            migrationBuilder.DropColumn(
                name: "CeMatrixParams",
                table: "CeMatrixResults");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "CeMatrixResults");

            migrationBuilder.AddColumn<int>(
                name: "BaseActuatorId",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagType",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "SafetyControllers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SafetyControllers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CeMatrixId",
                table: "CeMatrixResults",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "CeMatrices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CeMatrices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "BaseActuators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "BaseActuators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BaseActuators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_BaseActuatorId",
                table: "Tags",
                column: "BaseActuatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixResults_CeMatrixId",
                table: "CeMatrixResults",
                column: "CeMatrixId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators",
                column: "BaseActuatorTypeId",
                principalTable: "BaseActuatorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices",
                column: "CeMatrixTypeId",
                principalTable: "CeMatrixTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrixResults_CeMatrices_CeMatrixId",
                table: "CeMatrixResults",
                column: "CeMatrixId",
                principalTable: "CeMatrices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers",
                column: "SafetyControllerTypeId",
                principalTable: "SafetyControllerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_BaseActuators_BaseActuatorId",
                table: "Tags",
                column: "BaseActuatorId",
                principalTable: "BaseActuators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
