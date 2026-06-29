using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class DbSimplifying : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectVersions_ProjectVersionTypes_ProjectVersionTypeId",
                table: "ProjectVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_TagConditionInfos_TagTypes_TagTypeId",
                table: "TagConditionInfos");

            migrationBuilder.DropTable(
                name: "ActuatorParams");

            migrationBuilder.DropTable(
                name: "BaseActuatorTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "BasePcObjectTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "CeMatrixTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "SafetyControllerTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "TagTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "BaseActuatorTypes");

            migrationBuilder.DropTable(
                name: "BasePcObjectTypes");

            migrationBuilder.DropTable(
                name: "CeMatrixTypes");

            migrationBuilder.DropTable(
                name: "SafetyControllerTypes");

            migrationBuilder.DropTable(
                name: "TagTypes");

            migrationBuilder.DropIndex(
                name: "IX_TagConditionInfos_TagTypeId",
                table: "TagConditionInfos");

            migrationBuilder.DropIndex(
                name: "IX_ProjectVersions_ProjectVersionTypeId",
                table: "ProjectVersions");

            migrationBuilder.DropColumn(
                name: "TagTypeId",
                table: "TagConditionInfos");

            migrationBuilder.DropColumn(
                name: "ProjectVersionTypeId",
                table: "ProjectVersions");

            migrationBuilder.DropColumn(
                name: "CanBeCause",
                table: "Legends");

            migrationBuilder.DropColumn(
                name: "CanBeEffect",
                table: "Legends");

            migrationBuilder.DropColumn(
                name: "Desc",
                table: "Legends");

            migrationBuilder.AddColumn<string>(
                name: "ProjectVersionType",
                table: "ProjectVersions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "ParamDescs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LegendDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LegendId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false),
                    LastWriteTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BytesCount = table.Column<int>(type: "integer", nullable: false),
                    FileBytesHash_Base64 = table.Column<string>(type: "text", nullable: true),
                    DbFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegendDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegendDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LegendDbFileReferences_Legends_LegendId",
                        column: x => x.LegendId,
                        principalTable: "Legends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegendParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LegendId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegendParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegendParams_Legends_LegendId",
                        column: x => x.LegendId,
                        principalTable: "Legends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegendDbFileReferences_DbFileId",
                table: "LegendDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_LegendDbFileReferences_LegendId_Path_Name",
                table: "LegendDbFileReferences",
                columns: new[] { "LegendId", "Path", "Name" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegendParams_LegendId_ParamName",
                table: "LegendParams",
                columns: new[] { "LegendId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegendDbFileReferences");

            migrationBuilder.DropTable(
                name: "LegendParams");

            migrationBuilder.DropColumn(
                name: "ProjectVersionType",
                table: "ProjectVersions");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "ParamDescs");

            migrationBuilder.AddColumn<int>(
                name: "TagTypeId",
                table: "TagConditionInfos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectVersionTypeId",
                table: "ProjectVersions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CanBeCause",
                table: "Legends",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanBeEffect",
                table: "Legends",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Desc",
                table: "Legends",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ActuatorParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActuatorParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActuatorParams_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseActuatorTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseActuatorTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseActuatorTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BasePcObjectTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePcObjectTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePcObjectTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeMatrixTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrixTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SafetyControllerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyControllerTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyControllerTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TagTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseActuatorTypes_ParamInfos",
                columns: table => new
                {
                    BaseActuatorTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseActuatorTypes_ParamInfos", x => new { x.BaseActuatorTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_BaseActuatorTypes_ParamInfos_BaseActuatorTypes_BaseActuator~",
                        column: x => x.BaseActuatorTypesId,
                        principalTable: "BaseActuatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseActuatorTypes_ParamInfos_ParamInfos_StandardParamInfosId",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasePcObjectTypes_ParamInfos",
                columns: table => new
                {
                    BasePcObjectTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePcObjectTypes_ParamInfos", x => new { x.BasePcObjectTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_BasePcObjectTypes_ParamInfos_BasePcObjectTypes_BasePcObject~",
                        column: x => x.BasePcObjectTypesId,
                        principalTable: "BasePcObjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BasePcObjectTypes_ParamInfos_ParamInfos_StandardParamInfosId",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixTypes_ParamInfos",
                columns: table => new
                {
                    CeMatrixTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeMatrixTypes_ParamInfos", x => new { x.CeMatrixTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_CeMatrixTypes_ParamInfos_CeMatrixTypes_CeMatrixTypesId",
                        column: x => x.CeMatrixTypesId,
                        principalTable: "CeMatrixTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CeMatrixTypes_ParamInfos_ParamInfos_StandardParamInfosId",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyControllerTypes_ParamInfos",
                columns: table => new
                {
                    SafetyControllerTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyControllerTypes_ParamInfos", x => new { x.SafetyControllerTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_SafetyControllerTypes_ParamInfos_ParamInfos_StandardParamIn~",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafetyControllerTypes_ParamInfos_SafetyControllerTypes_Safe~",
                        column: x => x.SafetyControllerTypesId,
                        principalTable: "SafetyControllerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagTypes_ParamInfos",
                columns: table => new
                {
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false),
                    TagTypesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypes_ParamInfos", x => new { x.StandardParamInfosId, x.TagTypesId });
                    table.ForeignKey(
                        name: "FK_TagTypes_ParamInfos_ParamInfos_StandardParamInfosId",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagTypes_ParamInfos_TagTypes_TagTypesId",
                        column: x => x.TagTypesId,
                        principalTable: "TagTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagConditionInfos_TagTypeId",
                table: "TagConditionInfos",
                column: "TagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersions_ProjectVersionTypeId",
                table: "ProjectVersions",
                column: "ProjectVersionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActuatorParams_TagId_ParamName",
                table: "ActuatorParams",
                columns: new[] { "TagId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorTypes_IconDbFileId",
                table: "BaseActuatorTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorTypes_Type",
                table: "BaseActuatorTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorTypes_ParamInfos_StandardParamInfosId",
                table: "BaseActuatorTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectTypes_IconDbFileId",
                table: "BasePcObjectTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectTypes_Type",
                table: "BasePcObjectTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectTypes_ParamInfos_StandardParamInfosId",
                table: "BasePcObjectTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixTypes_IconDbFileId",
                table: "CeMatrixTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixTypes_Type",
                table: "CeMatrixTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixTypes_ParamInfos_StandardParamInfosId",
                table: "CeMatrixTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerTypes_IconDbFileId",
                table: "SafetyControllerTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerTypes_Type",
                table: "SafetyControllerTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerTypes_ParamInfos_StandardParamInfosId",
                table: "SafetyControllerTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTypes_IconDbFileId",
                table: "TagTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TagTypes_Type",
                table: "TagTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagTypes_ParamInfos_TagTypesId",
                table: "TagTypes_ParamInfos",
                column: "TagTypesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectVersions_ProjectVersionTypes_ProjectVersionTypeId",
                table: "ProjectVersions",
                column: "ProjectVersionTypeId",
                principalTable: "ProjectVersionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagConditionInfos_TagTypes_TagTypeId",
                table: "TagConditionInfos",
                column: "TagTypeId",
                principalTable: "TagTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
