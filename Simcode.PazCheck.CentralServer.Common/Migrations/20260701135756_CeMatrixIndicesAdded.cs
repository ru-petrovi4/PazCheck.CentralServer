using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class CeMatrixIndicesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId",
                table: "Cells");

            migrationBuilder.CreateIndex(
                name: "IX_Rows_CeMatrixId__CreateProjectVersionNum",
                table: "Rows",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" });

            migrationBuilder.CreateIndex(
                name: "IX_Rows_CeMatrixId__DeleteProjectVersionNum",
                table: "Rows",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" });

            migrationBuilder.CreateIndex(
                name: "IX_Columns_CeMatrixId__CreateProjectVersionNum",
                table: "Columns",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" });

            migrationBuilder.CreateIndex(
                name: "IX_Columns_CeMatrixId__DeleteProjectVersionNum",
                table: "Columns",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" });

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId__CreateProjectVersionNum",
                table: "Cells",
                columns: new[] { "CeMatrixId", "_CreateProjectVersionNum" });

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId__DeleteProjectVersionNum",
                table: "Cells",
                columns: new[] { "CeMatrixId", "_DeleteProjectVersionNum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rows_CeMatrixId__CreateProjectVersionNum",
                table: "Rows");

            migrationBuilder.DropIndex(
                name: "IX_Rows_CeMatrixId__DeleteProjectVersionNum",
                table: "Rows");

            migrationBuilder.DropIndex(
                name: "IX_Columns_CeMatrixId__CreateProjectVersionNum",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_CeMatrixId__DeleteProjectVersionNum",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId__CreateProjectVersionNum",
                table: "Cells");

            migrationBuilder.DropIndex(
                name: "IX_Cells_CeMatrixId__DeleteProjectVersionNum",
                table: "Cells");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId",
                table: "Cells",
                column: "CeMatrixId");
        }
    }
}
