using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class TagConditionIndexUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagConditions_TagId_ConditionCategory_AeCondition_DaConditi~",
                table: "TagConditions");

            migrationBuilder.CreateIndex(
                name: "IX_TagConditions_TagId_ConditionCategory_AeCondition_DaConditi~",
                table: "TagConditions",
                columns: new[] { "TagId", "ConditionCategory", "AeCondition", "DaCondition", "SymbolToDisplay" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TagConditions_TagId_ConditionCategory_AeCondition_DaConditi~",
                table: "TagConditions");

            migrationBuilder.CreateIndex(
                name: "IX_TagConditions_TagId_ConditionCategory_AeCondition_DaConditi~",
                table: "TagConditions",
                columns: new[] { "TagId", "ConditionCategory", "AeCondition", "DaCondition" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }
    }
}
