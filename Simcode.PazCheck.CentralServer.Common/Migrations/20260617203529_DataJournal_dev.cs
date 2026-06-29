using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class DataJournal_dev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CurrentValue_TimestampUtc",
                table: "JournalParamValuesCollections",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValue_TimestampUtc",
                table: "JournalParamValuesCollections");
        }
    }
}
