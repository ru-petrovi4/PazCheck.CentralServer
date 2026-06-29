using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class PcObjectK_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "K",
                table: "PcObjects",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "K",
                table: "PcObjects");
        }
    }
}
