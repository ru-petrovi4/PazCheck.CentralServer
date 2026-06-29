using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MonitoringSimplified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators");

            migrationBuilder.DropForeignKey(
                name: "FK_BasePcObjects_BasePcObjectTypes_BasePcObjectTypeId",
                table: "BasePcObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices");

            migrationBuilder.DropForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_SafetyControllers_SafetyControllerTypeId",
                table: "SafetyControllers");

            migrationBuilder.DropIndex(
                name: "IX_CeMatrices_CeMatrixTypeId",
                table: "CeMatrices");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_BasePcObjectTypeId",
                table: "BasePcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BaseActuators_BaseActuatorTypeId",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "SafetyControllerTypeId",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "CeMatrixTypeId",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "BasePcObjectTypeId",
                table: "BasePcObjects");

            migrationBuilder.DropColumn(
                name: "BaseActuatorTypeId",
                table: "BaseActuators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SafetyControllerTypeId",
                table: "SafetyControllers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CeMatrixTypeId",
                table: "CeMatrices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BasePcObjectTypeId",
                table: "BasePcObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseActuatorTypeId",
                table: "BaseActuators",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_SafetyControllerTypeId",
                table: "SafetyControllers",
                column: "SafetyControllerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_CeMatrixTypeId",
                table: "CeMatrices",
                column: "CeMatrixTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_BasePcObjectTypeId",
                table: "BasePcObjects",
                column: "BasePcObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_BaseActuatorTypeId",
                table: "BaseActuators",
                column: "BaseActuatorTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                table: "BaseActuators",
                column: "BaseActuatorTypeId",
                principalTable: "BaseActuatorTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BasePcObjects_BasePcObjectTypes_BasePcObjectTypeId",
                table: "BasePcObjects",
                column: "BasePcObjectTypeId",
                principalTable: "BasePcObjectTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                table: "CeMatrices",
                column: "CeMatrixTypeId",
                principalTable: "CeMatrixTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                table: "SafetyControllers",
                column: "SafetyControllerTypeId",
                principalTable: "SafetyControllerTypes",
                principalColumn: "Id");
        }
    }
}
