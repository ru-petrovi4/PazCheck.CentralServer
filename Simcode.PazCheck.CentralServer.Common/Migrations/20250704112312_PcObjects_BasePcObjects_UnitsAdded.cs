using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class PcObjects_BasePcObjects_UnitsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "PcObjects",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "BasePcObjects",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            // Обновляем все существующие записи
            migrationBuilder.Sql(@"
    UPDATE ""PcObjects""
    SET ""UnitId"" = (
        SELECT ""Id"" FROM ""Units"" WHERE ""IdentifierLower"" = 'mlsp'
        LIMIT 1
    );");
            migrationBuilder.Sql(@"
    UPDATE ""BasePcObjects""
    SET ""UnitId"" = (
        SELECT ""Id"" FROM ""Units"" WHERE ""IdentifierLower"" = 'mlsp'
        LIMIT 1
    );");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "PcObjects",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "BasePcObjects",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_UnitId",
                table: "PcObjects",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_UnitId",
                table: "BasePcObjects",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasePcObjects_Units_UnitId",
                table: "BasePcObjects",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PcObjects_Units_UnitId",
                table: "PcObjects",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasePcObjects_Units_UnitId",
                table: "BasePcObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PcObjects_Units_UnitId",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_PcObjects_UnitId",
                table: "PcObjects");

            migrationBuilder.DropIndex(
                name: "IX_BasePcObjects_UnitId",
                table: "BasePcObjects");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "PcObjects");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "BasePcObjects");
        }
    }
}
