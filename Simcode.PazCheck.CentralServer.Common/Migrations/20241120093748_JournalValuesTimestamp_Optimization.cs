using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class JournalValuesTimestamp_Optimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"TRUNCATE ""FloatJournalParamValues"";");

            migrationBuilder.DropColumn(
                name: "TimestampUtc",
                table: "FloatJournalParamValues");

            migrationBuilder.AddColumn<long>(
                name: "TimestampUtc",
                table: "FloatJournalParamValues",
                type: "bigint",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimestampUtc",
                table: "FloatJournalParamValues",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
