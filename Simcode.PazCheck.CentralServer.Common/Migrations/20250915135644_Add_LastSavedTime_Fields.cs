using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class Add_LastSavedTime_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "Tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "TagParams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "TagParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "TagDbFileReferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "TagDbFileReferences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "TagConditions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "TagConditions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "SafetyControllers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllerParams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "SafetyControllerParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllerDbFileReferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "SafetyControllerDbFileReferences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "Rows",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "Rows",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "Legends",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "Legends",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "LegendParams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "LegendParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "LegendDbFileReferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "LegendDbFileReferences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "Columns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "Columns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixParams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "CeMatrixParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixDbFileReferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "CeMatrixDbFileReferences",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "CeMatrixComments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "CeMatrices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "Cells",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "Cells",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "BaseActuators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuatorParams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "BaseActuatorParams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuatorDbFileReferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "_LastSavedChangeUser",
                table: "BaseActuatorDbFileReferences",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "TagParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "TagParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "TagDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "TagDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "TagConditions");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "TagConditions");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "SafetyControllers");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllerParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "SafetyControllerParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "SafetyControllerDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "SafetyControllerDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "Rows");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "Rows");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "Legends");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "Legends");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "LegendParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "LegendParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "LegendDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "LegendDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "CeMatrixParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "CeMatrixDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrixComments");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "CeMatrixComments");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "CeMatrices");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "Cells");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "Cells");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "BaseActuators");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuatorParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "BaseActuatorParams");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeTimeUtc",
                table: "BaseActuatorDbFileReferences");

            migrationBuilder.DropColumn(
                name: "_LastSavedChangeUser",
                table: "BaseActuatorDbFileReferences");
        }
    }
}
