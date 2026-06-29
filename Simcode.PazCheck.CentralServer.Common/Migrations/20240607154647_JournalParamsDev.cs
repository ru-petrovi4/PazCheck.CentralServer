using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class JournalParamsDev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloatJournalParamValues_PcObjectJournalParams_JournalParamId",
                table: "FloatJournalParamValues");

            migrationBuilder.DropForeignKey(
                name: "FK_Int32JournalParamValues_PcObjectJournalParams_JournalParamId",
                table: "Int32JournalParamValues");

            migrationBuilder.DropForeignKey(
                name: "FK_StringJournalParamValues_PcObjectJournalParams_JournalParam~",
                table: "StringJournalParamValues");

            migrationBuilder.DropColumn(
                name: "TypeCode",
                table: "PcObjectJournalParams");

            migrationBuilder.RenameColumn(
                name: "JournalParamId",
                table: "StringJournalParamValues",
                newName: "JournalParamValuesCollectionId");

            migrationBuilder.RenameColumn(
                name: "JournalParamId",
                table: "Int32JournalParamValues",
                newName: "JournalParamValuesCollectionId");

            migrationBuilder.RenameColumn(
                name: "JournalParamId",
                table: "FloatJournalParamValues",
                newName: "JournalParamValuesCollectionId");

            migrationBuilder.CreateTable(
                name: "JournalParamValuesCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    MetadataFields = table.Column<string>(type: "text", nullable: false),
                    TypeCode = table.Column<byte>(type: "smallint", nullable: false),
                    PcObjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalParamValuesCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalParamValuesCollections_PcObjects_PcObjectId",
                        column: x => x.PcObjectId,
                        principalTable: "PcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalParamValuesCollections_PcObjectId",
                table: "JournalParamValuesCollections",
                column: "PcObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloatJournalParamValues_JournalParamValuesCollections_Journ~",
                table: "FloatJournalParamValues",
                column: "JournalParamValuesCollectionId",
                principalTable: "JournalParamValuesCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Int32JournalParamValues_JournalParamValuesCollections_Journ~",
                table: "Int32JournalParamValues",
                column: "JournalParamValuesCollectionId",
                principalTable: "JournalParamValuesCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StringJournalParamValues_JournalParamValuesCollections_Jour~",
                table: "StringJournalParamValues",
                column: "JournalParamValuesCollectionId",
                principalTable: "JournalParamValuesCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloatJournalParamValues_JournalParamValuesCollections_Journ~",
                table: "FloatJournalParamValues");

            migrationBuilder.DropForeignKey(
                name: "FK_Int32JournalParamValues_JournalParamValuesCollections_Journ~",
                table: "Int32JournalParamValues");

            migrationBuilder.DropForeignKey(
                name: "FK_StringJournalParamValues_JournalParamValuesCollections_Jour~",
                table: "StringJournalParamValues");

            migrationBuilder.DropTable(
                name: "JournalParamValuesCollections");

            migrationBuilder.RenameColumn(
                name: "JournalParamValuesCollectionId",
                table: "StringJournalParamValues",
                newName: "JournalParamId");

            migrationBuilder.RenameColumn(
                name: "JournalParamValuesCollectionId",
                table: "Int32JournalParamValues",
                newName: "JournalParamId");

            migrationBuilder.RenameColumn(
                name: "JournalParamValuesCollectionId",
                table: "FloatJournalParamValues",
                newName: "JournalParamId");

            migrationBuilder.AddColumn<byte>(
                name: "TypeCode",
                table: "PcObjectJournalParams",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddForeignKey(
                name: "FK_FloatJournalParamValues_PcObjectJournalParams_JournalParamId",
                table: "FloatJournalParamValues",
                column: "JournalParamId",
                principalTable: "PcObjectJournalParams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Int32JournalParamValues_PcObjectJournalParams_JournalParamId",
                table: "Int32JournalParamValues",
                column: "JournalParamId",
                principalTable: "PcObjectJournalParams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StringJournalParamValues_PcObjectJournalParams_JournalParam~",
                table: "StringJournalParamValues",
                column: "JournalParamId",
                principalTable: "PcObjectJournalParams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
