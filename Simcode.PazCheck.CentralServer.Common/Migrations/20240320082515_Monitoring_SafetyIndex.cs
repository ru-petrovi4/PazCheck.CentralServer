using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class Monitoring_SafetyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AlarmIntensity",
                table: "PcObjects",
                newName: "SafetyIndex");

            migrationBuilder.RenameColumn(
                name: "AlarmDesc",
                table: "PcObjects",
                newName: "SafetyIndexDesc");

            migrationBuilder.RenameColumn(
                name: "AlarmChangedTimeUtc",
                table: "PcObjects",
                newName: "SafetyIndex_LastChangeTimeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SafetyIndex_LastChangeTimeUtc",
                table: "PcObjects",
                newName: "AlarmChangedTimeUtc");

            migrationBuilder.RenameColumn(
                name: "SafetyIndexDesc",
                table: "PcObjects",
                newName: "AlarmDesc");

            migrationBuilder.RenameColumn(
                name: "SafetyIndex",
                table: "PcObjects",
                newName: "AlarmIntensity");
        }
    }
}
