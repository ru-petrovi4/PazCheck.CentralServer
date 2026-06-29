using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class MetaInfo_Table_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "InformationSecurityEvents",
                type: "character varying(55)",
                maxLength: 55,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "MetaParams",
                columns: table => new
                {
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaParams", x => x.ParamName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaParams");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "InformationSecurityEvents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(55)",
                oldMaxLength: 55);
        }
    }
}
