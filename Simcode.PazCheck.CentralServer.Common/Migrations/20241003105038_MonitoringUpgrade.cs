using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MonitoringUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PcObjectEvents_PcObjectEventTypes_PcObjectEventTypeId",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_PcObjectEvents_PcObjectEventTypeId",
                table: "PcObjectEvents");

            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections");

            migrationBuilder.DropColumn(
                name: "Calculation",
                table: "PcObjects");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "PcObjects");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PcObjects");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "PcObjectEvents");

            migrationBuilder.DropColumn(
                name: "PcObjectEventTypeId",
                table: "PcObjectEvents");

            migrationBuilder.DropColumn(
                name: "Calculation",
                table: "BasePcObjects");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "BasePcObjects");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "BasePcObjects");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "PcObjectEvents",
                newName: "PcObjectEventType");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections",
                columns: new[] { "PcObjectId", "ParamName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections");

            migrationBuilder.RenameColumn(
                name: "PcObjectEventType",
                table: "PcObjectEvents",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Calculation",
                table: "PcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "PcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "PcObjectEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PcObjectEventTypeId",
                table: "PcObjectEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Calculation",
                table: "BasePcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "BasePcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BasePcObjects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectEventTypeId",
                table: "PcObjectEvents",
                column: "PcObjectEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId_ParamName",
                table: "JournalParamValuesCollections",
                columns: new[] { "PcObjectId", "ParamName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PcObjectEvents_PcObjectEventTypes_PcObjectEventTypeId",
                table: "PcObjectEvents",
                column: "PcObjectEventTypeId",
                principalTable: "PcObjectEventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
