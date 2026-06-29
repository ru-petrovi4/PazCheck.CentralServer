using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MetaParamsRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HubMethod",
                table: "MetaParams",
                newName: "Method");

            migrationBuilder.RenameColumn(
                name: "HubGroup",
                table: "MetaParams",
                newName: "Group");

            migrationBuilder.RenameColumn(
                name: "HasHubArg",
                table: "MetaParams",
                newName: "HasArg");

            migrationBuilder.RenameColumn(
                name: "HubArg",
                table: "MetaParamHubArgs",
                newName: "Arg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Method",
                table: "MetaParams",
                newName: "HubMethod");

            migrationBuilder.RenameColumn(
                name: "HasArg",
                table: "MetaParams",
                newName: "HasHubArg");

            migrationBuilder.RenameColumn(
                name: "Group",
                table: "MetaParams",
                newName: "HubGroup");

            migrationBuilder.RenameColumn(
                name: "Arg",
                table: "MetaParamHubArgs",
                newName: "HubArg");
        }
    }
}
