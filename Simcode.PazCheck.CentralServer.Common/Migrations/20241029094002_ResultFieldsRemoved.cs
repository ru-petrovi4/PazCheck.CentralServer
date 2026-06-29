using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class ResultFieldsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagCondition_AeCondition",
                table: "RowResults");

            migrationBuilder.DropColumn(
                name: "TagCondition_DaCondition",
                table: "RowResults");

            migrationBuilder.DropColumn(
                name: "TagCondition_AeCondition",
                table: "ColumnResults");

            migrationBuilder.DropColumn(
                name: "TagCondition_DaCondition",
                table: "ColumnResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagCondition_AeCondition",
                table: "RowResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagCondition_DaCondition",
                table: "RowResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagCondition_AeCondition",
                table: "ColumnResults",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TagCondition_DaCondition",
                table: "ColumnResults",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
