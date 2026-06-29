using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MetaParamsRefactoring2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaParamHubArgs",
                table: "MetaParamHubArgs");

            migrationBuilder.RenameTable(
                name: "MetaParamHubArgs",
                newName: "MetaParamArgs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaParamArgs",
                table: "MetaParamArgs",
                column: "ParamName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MetaParamArgs",
                table: "MetaParamArgs");

            migrationBuilder.RenameTable(
                name: "MetaParamArgs",
                newName: "MetaParamHubArgs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MetaParamHubArgs",
                table: "MetaParamHubArgs",
                column: "ParamName");
        }
    }
}
