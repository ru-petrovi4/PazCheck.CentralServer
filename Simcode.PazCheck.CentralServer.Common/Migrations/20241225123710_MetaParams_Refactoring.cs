using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MetaParams_Refactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSingleton",
                table: "MetaParams");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MetaParams",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "MetaParams");

            migrationBuilder.AddColumn<bool>(
                name: "IsSingleton",
                table: "MetaParams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
