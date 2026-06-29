using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class TagIndexAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjectId__CreateProjectVersionNum",
                table: "Tags",
                columns: new[] { "ProjectId", "_CreateProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjectId__DeleteProjectVersionNum",
                table: "Tags",
                columns: new[] { "ProjectId", "_DeleteProjectVersionNum" },
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_ProjectId__CreateProjectVersionNum",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_ProjectId__DeleteProjectVersionNum",
                table: "Tags");
        }
    }
}
