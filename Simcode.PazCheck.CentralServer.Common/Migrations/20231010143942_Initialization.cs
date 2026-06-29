using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Simcode.PazCheck.CentralServer.Common.Migrations
{
    /// <inheritdoc />
    public partial class Initialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddonStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourcePath = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<string>(type: "text", nullable: false),
                    SourceIdToDisplay = table.Column<string>(type: "text", nullable: false),
                    AddonGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    AddonIdentifier = table.Column<string>(type: "text", nullable: false),
                    AddonInstanceId = table.Column<string>(type: "text", nullable: false),
                    LastWorkTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StateCode = table.Column<long>(type: "bigint", nullable: false),
                    Info = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddonStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    ValueEncrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    ValueHash_Base64 = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbFileContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileBytes_Base64 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbFileContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InformationSecurityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    EventIdDesc = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    SeverityDesc = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    SourceIpAddress = table.Column<string>(type: "text", nullable: false),
                    SourceHost = table.Column<string>(type: "text", nullable: false),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    EventSubject = table.Column<string>(type: "text", nullable: false),
                    EventObject = table.Column<string>(type: "text", nullable: false),
                    EventAdditionalFields = table.Column<string>(type: "text", nullable: false),
                    EventDesc = table.Column<string>(type: "text", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationSecurityEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    JobTitle = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    ProgressPercent = table.Column<long>(type: "bigint", nullable: false),
                    ProgressLabel = table.Column<string>(type: "text", nullable: false),
                    ProgressDetail = table.Column<string>(type: "text", nullable: false),
                    JobStatusCode = table.Column<long>(type: "bigint", nullable: false),
                    BeginTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParamDescs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParamDescs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParamInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: false),
                    MetadataFields = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParamInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestData = table.Column<string>(type: "text", nullable: false),
                    RequestUser = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ReplyUser = table.Column<string>(type: "text", nullable: false),
                    ReplyMessage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleApiFunctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Modifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleApiFunctions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleBusinessFunctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleBusinessFunctions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JobId = table.Column<string>(type: "text", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    LogLevel = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    _DeleteTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileBytesCount = table.Column<int>(type: "integer", nullable: false),
                    FileBytesHash_Base64 = table.Column<string>(type: "text", nullable: false),
                    DbFileContentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbFiles_DbFileContents_DbFileContentId",
                        column: x => x.DbFileContentId,
                        principalTable: "DbFileContents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InformationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User = table.Column<string>(type: "text", nullable: false),
                    LogLevel = table.Column<int>(type: "integer", nullable: false),
                    TimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    Acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    RelatedRequestMessageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InformationMessages_RequestMessages_RelatedRequestMessageId",
                        column: x => x.RelatedRequestMessageId,
                        principalTable: "RequestMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleApiFunctionRoleBusinessFunction",
                columns: table => new
                {
                    RoleApiFunctionsId = table.Column<int>(type: "integer", nullable: false),
                    RoleBusinessFunctionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleApiFunctionRoleBusinessFunction", x => new { x.RoleApiFunctionsId, x.RoleBusinessFunctionsId });
                    table.ForeignKey(
                        name: "FK_RoleApiFunctionRoleBusinessFunction_RoleApiFunctions_RoleAp~",
                        column: x => x.RoleApiFunctionsId,
                        principalTable: "RoleApiFunctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleApiFunctionRoleBusinessFunction_RoleBusinessFunctions_R~",
                        column: x => x.RoleBusinessFunctionsId,
                        principalTable: "RoleBusinessFunctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleBusinessFunctionId = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    IsAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_RoleBusinessFunctions_RoleBusinessFunctionId",
                        column: x => x.RoleBusinessFunctionId,
                        principalTable: "RoleBusinessFunctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaseActuatorTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
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
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
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
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
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
                name: "LicenseFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    DbFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseFiles_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PcObjectEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcObjectEventTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjectEventTypes_DbFiles_IconDbFileId",
                        column: x => x.IconDbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersionTypes_DbFiles_IconDbFileId",
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
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
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
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    IconDbFileId = table.Column<int>(type: "integer", nullable: true)
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
                name: "BasePcObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Calculation = table.Column<string>(type: "text", nullable: false),
                    Widgets = table.Column<string>(type: "text", nullable: false),
                    Params = table.Column<string>(type: "text", nullable: false),
                    BasePcObjectTypeId = table.Column<int>(type: "integer", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePcObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePcObjects_BasePcObjectTypes_BasePcObjectTypeId",
                        column: x => x.BasePcObjectTypeId,
                        principalTable: "BasePcObjectTypes",
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
                name: "PcObjectEventTypes_ParamInfos",
                columns: table => new
                {
                    PcObjectEventTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcObjectEventTypes_ParamInfos", x => new { x.PcObjectEventTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_PcObjectEventTypes_ParamInfos_ParamInfos_StandardParamInfos~",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PcObjectEventTypes_ParamInfos_PcObjectEventTypes_PcObjectEv~",
                        column: x => x.PcObjectEventTypesId,
                        principalTable: "PcObjectEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersionTypes_ParamInfos",
                columns: table => new
                {
                    ProjectVersionTypesId = table.Column<int>(type: "integer", nullable: false),
                    StandardParamInfosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersionTypes_ParamInfos", x => new { x.ProjectVersionTypesId, x.StandardParamInfosId });
                    table.ForeignKey(
                        name: "FK_ProjectVersionTypes_ParamInfos_ParamInfos_StandardParamInfo~",
                        column: x => x.StandardParamInfosId,
                        principalTable: "ParamInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectVersionTypes_ParamInfos_ProjectVersionTypes_ProjectV~",
                        column: x => x.ProjectVersionTypesId,
                        principalTable: "ProjectVersionTypes",
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
                name: "TagConditionInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AeCondition = table.Column<string>(type: "text", nullable: false),
                    DaCondition = table.Column<string>(type: "text", nullable: false),
                    SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    CanBeCause = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeEffect = table.Column<bool>(type: "boolean", nullable: false),
                    TagTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagConditionInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagConditionInfos_TagTypes_TagTypeId",
                        column: x => x.TagTypeId,
                        principalTable: "TagTypes",
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

            migrationBuilder.CreateTable(
                name: "BasePcObjectDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BasePcObjectId = table.Column<int>(type: "integer", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BasePcObjectDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePcObjectDbFileReferences_BasePcObjects_BasePcObjectId",
                        column: x => x.BasePcObjectId,
                        principalTable: "BasePcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BasePcObjectDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BasePcObjectJournalParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BasePcObjectId = table.Column<int>(type: "integer", nullable: false),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    MetadataFields = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePcObjectJournalParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasePcObjectJournalParams_BasePcObjects_BasePcObjectId",
                        column: x => x.BasePcObjectId,
                        principalTable: "BasePcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasePcObjects_EventTypes",
                columns: table => new
                {
                    BasePcObjectsId = table.Column<int>(type: "integer", nullable: false),
                    PcObjectEventTypesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePcObjects_EventTypes", x => new { x.BasePcObjectsId, x.PcObjectEventTypesId });
                    table.ForeignKey(
                        name: "FK_BasePcObjects_EventTypes_BasePcObjects_BasePcObjectsId",
                        column: x => x.BasePcObjectsId,
                        principalTable: "BasePcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BasePcObjects_EventTypes_PcObjectEventTypes_PcObjectEventTy~",
                        column: x => x.PcObjectEventTypesId,
                        principalTable: "PcObjectEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Calculation = table.Column<string>(type: "text", nullable: false),
                    Widgets = table.Column<string>(type: "text", nullable: false),
                    BasePcObjectId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    AlarmLevel = table.Column<int>(type: "integer", nullable: false),
                    AlarmIntensity = table.Column<double>(type: "double precision", nullable: false),
                    K = table.Column<double>(type: "double precision", nullable: false),
                    Params = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjects_BasePcObjects_BasePcObjectId",
                        column: x => x.BasePcObjectId,
                        principalTable: "BasePcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PcObjects_PcObjects_ParentId",
                        column: x => x.ParentId,
                        principalTable: "PcObjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PcObjectDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PcObjectId = table.Column<int>(type: "integer", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_PcObjectDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjectDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcObjectDbFileReferences_PcObjects_PcObjectId",
                        column: x => x.PcObjectId,
                        principalTable: "PcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcObjectEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BeginTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Params = table.Column<string>(type: "text", nullable: false),
                    PcObjectEventTypeId = table.Column<int>(type: "integer", nullable: false),
                    PcObjectId = table.Column<int>(type: "integer", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcObjectEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjectEvents_PcObjectEventTypes_PcObjectEventTypeId",
                        column: x => x.PcObjectEventTypeId,
                        principalTable: "PcObjectEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PcObjectEvents_PcObjects_PcObjectId",
                        column: x => x.PcObjectId,
                        principalTable: "PcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcObjectJournalParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TypeCode = table.Column<byte>(type: "smallint", nullable: false),
                    PcObjectId = table.Column<int>(type: "integer", nullable: false),
                    ParamName = table.Column<string>(type: "text", nullable: false),
                    MetadataFields = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcObjectJournalParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjectJournalParams_PcObjects_PcObjectId",
                        column: x => x.PcObjectId,
                        principalTable: "PcObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcObjectEventDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PcObjectEventId = table.Column<int>(type: "integer", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_PcObjectEventDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcObjectEventDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcObjectEventDbFileReferences_PcObjectEvents_PcObjectEventId",
                        column: x => x.PcObjectEventId,
                        principalTable: "PcObjectEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloatJournalParamValues",
                columns: table => new
                {
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JournalParamId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloatJournalParamValues", x => new { x.JournalParamId, x.TimestampUtc });
                    table.ForeignKey(
                        name: "FK_FloatJournalParamValues_PcObjectJournalParams_JournalParamId",
                        column: x => x.JournalParamId,
                        principalTable: "PcObjectJournalParams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Int32JournalParamValues",
                columns: table => new
                {
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JournalParamId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Int32JournalParamValues", x => new { x.JournalParamId, x.TimestampUtc });
                    table.ForeignKey(
                        name: "FK_Int32JournalParamValues_PcObjectJournalParams_JournalParamId",
                        column: x => x.JournalParamId,
                        principalTable: "PcObjectJournalParams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StringJournalParamValues",
                columns: table => new
                {
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JournalParamId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringJournalParamValues", x => new { x.JournalParamId, x.TimestampUtc });
                    table.ForeignKey(
                        name: "FK_StringJournalParamValues_PcObjectJournalParams_JournalParam~",
                        column: x => x.JournalParamId,
                        principalTable: "PcObjectJournalParams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActuatorParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ActuatorParams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseActuatorDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseActuatorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BaseActuatorDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseActuatorDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseActuatorParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseActuatorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BaseActuatorParams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseActuators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    BaseActuatorTypeId = table.Column<int>(type: "integer", nullable: true),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    _LockedByUser = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseActuators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseActuators_BaseActuatorTypes_BaseActuatorTypeId",
                        column: x => x.BaseActuatorTypeId,
                        principalTable: "BaseActuatorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CellResultResultEvent",
                columns: table => new
                {
                    CellResultsId = table.Column<int>(type: "integer", nullable: false),
                    ResultEventsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellResultResultEvent", x => new { x.CellResultsId, x.ResultEventsId });
                });

            migrationBuilder.CreateTable(
                name: "CellResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowResultId = table.Column<int>(type: "integer", nullable: false),
                    ColumnResultId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    OutputTriggeredTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredType = table.Column<int>(type: "integer", nullable: false),
                    CeMatrixResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CellResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowId = table.Column<int>(type: "integer", nullable: false),
                    ColumnId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CeMatrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    CeMatrixTypeId = table.Column<int>(type: "integer", nullable: true),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    _LockedByUser = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeMatrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrices_CeMatrixTypes_CeMatrixTypeId",
                        column: x => x.CeMatrixTypeId,
                        principalTable: "CeMatrixTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeMatrixComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrixComments_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CeMatrixDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrixDbFileReferences_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CeMatrixDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CeMatrixParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrixParams_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Columns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<string>(type: "text", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    TagCondition_SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Columns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Columns_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<string>(type: "text", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    TagCondition_SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rows_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CeMatrixResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CeMatrixId = table.Column<int>(type: "integer", nullable: true),
                    ResultId = table.Column<int>(type: "integer", nullable: false),
                    Statistics = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeMatrixResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CeMatrixResults_CeMatrices_CeMatrixId",
                        column: x => x.CeMatrixId,
                        principalTable: "CeMatrices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ColumnResultResultEvent",
                columns: table => new
                {
                    ColumnResultsId = table.Column<int>(type: "integer", nullable: false),
                    ResultEventsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnResultResultEvent", x => new { x.ColumnResultsId, x.ResultEventsId });
                });

            migrationBuilder.CreateTable(
                name: "ColumnResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriggeredType = table.Column<int>(type: "integer", nullable: false),
                    TriggeredTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggeredUnitEventId = table.Column<int>(type: "integer", nullable: true),
                    MaxActuationTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    Order = table.Column<string>(type: "text", nullable: false),
                    IsDebug = table.Column<bool>(type: "boolean", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    TagCondition_AeCondition = table.Column<string>(type: "text", nullable: false),
                    TagCondition_DaCondition = table.Column<string>(type: "text", nullable: false),
                    TagCondition_SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    CeMatrixResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColumnResults_CeMatrixResults_CeMatrixResultId",
                        column: x => x.CeMatrixResultId,
                        principalTable: "CeMatrixResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Legends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    CanBeCause = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeEffect = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    _LockedByUser = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Legends", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionNum = table.Column<long>(type: "bigint", nullable: false),
                    TimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Active_TimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active_SupervisorUser = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProjectVersionTypeId = table.Column<int>(type: "integer", nullable: false),
                    Params = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersions_ProjectVersionTypes_ProjectVersionTypeId",
                        column: x => x.ProjectVersionTypeId,
                        principalTable: "ProjectVersionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectVersions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyControllers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    SafetyControllerTypeId = table.Column<int>(type: "integer", nullable: true),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    _LockedByUser = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyControllers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyControllers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SafetyControllers_SafetyControllerTypes_SafetyControllerTyp~",
                        column: x => x.SafetyControllerTypeId,
                        principalTable: "SafetyControllerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    TagType = table.Column<string>(type: "text", nullable: false),
                    BaseActuatorId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    _LockedByUser = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_BaseActuators_BaseActuatorId",
                        column: x => x.BaseActuatorId,
                        principalTable: "BaseActuators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tags_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersionDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectVersionId = table.Column<int>(type: "integer", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ProjectVersionDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersionDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectVersionDbFileReferences_ProjectVersions_ProjectVersi~",
                        column: x => x.ProjectVersionId,
                        principalTable: "ProjectVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    ActiveProjectVersionId = table.Column<int>(type: "integer", nullable: true),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_ProjectVersions_ActiveProjectVersionId",
                        column: x => x.ActiveProjectVersionId,
                        principalTable: "ProjectVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SafetyControllerDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SafetyControllerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SafetyControllerDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyControllerDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafetyControllerDbFileReferences_SafetyControllers_SafetyCo~",
                        column: x => x.SafetyControllerId,
                        principalTable: "SafetyControllers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SafetyControllerParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SafetyControllerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SafetyControllerParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyControllerParams_SafetyControllers_SafetyControllerId",
                        column: x => x.SafetyControllerId,
                        principalTable: "SafetyControllers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConditionCategory = table.Column<string>(type: "text", nullable: false),
                    AeCondition = table.Column<string>(type: "text", nullable: false),
                    DaCondition = table.Column<string>(type: "text", nullable: false),
                    CanBeCause = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeEffect = table.Column<bool>(type: "boolean", nullable: false),
                    SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    _CreateProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _DeleteProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    _HasUnversionedChanges = table.Column<bool>(type: "boolean", nullable: false),
                    _CreateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _CreateUser = table.Column<string>(type: "text", nullable: false),
                    _LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _LastChangeUser = table.Column<string>(type: "text", nullable: false),
                    _IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagConditions_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagDbFileReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TagDbFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagDbFileReferences_DbFiles_DbFileId",
                        column: x => x.DbFileId,
                        principalTable: "DbFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TagDbFileReferences_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagParams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TagParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagParams_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlalyzeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    BeginTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectVersionNum = table.Column<long>(type: "bigint", nullable: true),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Statistics = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitEventsIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    BeginTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitEventsIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitEventsIntervals_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TagName = table.Column<string>(type: "text", nullable: false),
                    ConditionString = table.Column<string>(type: "text", nullable: false),
                    ConditionIsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    OriginalEvent = table.Column<string>(type: "text", nullable: false),
                    UnitEventsIntervalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitEvents_UnitEventsIntervals_UnitEventsIntervalId",
                        column: x => x.UnitEventsIntervalId,
                        principalTable: "UnitEventsIntervals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ConditionCategory = table.Column<string>(type: "text", nullable: false),
                    AeCondition = table.Column<string>(type: "text", nullable: false),
                    DaCondition = table.Column<string>(type: "text", nullable: false),
                    TriggeredType = table.Column<int>(type: "integer", nullable: false),
                    TriggeredTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TriggeredUnitEventId = table.Column<int>(type: "integer", nullable: true),
                    NewValue = table.Column<bool>(type: "boolean", nullable: false),
                    Params = table.Column<string>(type: "text", nullable: false),
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultEvents_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultEvents_UnitEvents_TriggeredUnitEventId",
                        column: x => x.TriggeredUnitEventId,
                        principalTable: "UnitEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RowResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InputTriggeredTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InputTriggeredUnitEventId = table.Column<int>(type: "integer", nullable: true),
                    OutputTriggeredTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Order = table.Column<string>(type: "text", nullable: false),
                    IsDebug = table.Column<bool>(type: "boolean", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false),
                    TagCondition_AeCondition = table.Column<string>(type: "text", nullable: false),
                    TagCondition_DaCondition = table.Column<string>(type: "text", nullable: false),
                    TagCondition_SymbolToDisplay = table.Column<string>(type: "text", nullable: true),
                    CeMatrixResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RowResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RowResults_CeMatrixResults_CeMatrixResultId",
                        column: x => x.CeMatrixResultId,
                        principalTable: "CeMatrixResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RowResults_UnitEvents_InputTriggeredUnitEventId",
                        column: x => x.InputTriggeredUnitEventId,
                        principalTable: "UnitEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResultEventResultEvent",
                columns: table => new
                {
                    CauseResultEventsId = table.Column<int>(type: "integer", nullable: false),
                    EffectResultEventsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultEventResultEvent", x => new { x.CauseResultEventsId, x.EffectResultEventsId });
                    table.ForeignKey(
                        name: "FK_ResultEventResultEvent_ResultEvents_CauseResultEventsId",
                        column: x => x.CauseResultEventsId,
                        principalTable: "ResultEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultEventResultEvent_ResultEvents_EffectResultEventsId",
                        column: x => x.EffectResultEventsId,
                        principalTable: "ResultEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultEventRowResult",
                columns: table => new
                {
                    ResultEventsId = table.Column<int>(type: "integer", nullable: false),
                    RowResultsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultEventRowResult", x => new { x.ResultEventsId, x.RowResultsId });
                    table.ForeignKey(
                        name: "FK_ResultEventRowResult_ResultEvents_ResultEventsId",
                        column: x => x.ResultEventsId,
                        principalTable: "ResultEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultEventRowResult_RowResults_RowResultsId",
                        column: x => x.RowResultsId,
                        principalTable: "RowResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActuatorParams_TagId_ParamName",
                table: "ActuatorParams",
                columns: new[] { "TagId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AddonStatuses_SourcePath_AddonInstanceId",
                table: "AddonStatuses",
                columns: new[] { "SourcePath", "AddonInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorDbFileReferences_BaseActuatorId_Path_Name",
                table: "BaseActuatorDbFileReferences",
                columns: new[] { "BaseActuatorId", "Path", "Name" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorDbFileReferences_DbFileId",
                table: "BaseActuatorDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuatorParams_BaseActuatorId_ParamName",
                table: "BaseActuatorParams",
                columns: new[] { "BaseActuatorId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_BaseActuatorTypeId",
                table: "BaseActuators",
                column: "BaseActuatorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseActuators_ProjectId_Identifier",
                table: "BaseActuators",
                columns: new[] { "ProjectId", "Identifier" },
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
                name: "IX_BasePcObjectDbFileReferences_BasePcObjectId",
                table: "BasePcObjectDbFileReferences",
                column: "BasePcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectDbFileReferences_DbFileId",
                table: "BasePcObjectDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjectJournalParams_BasePcObjectId",
                table: "BasePcObjectJournalParams",
                column: "BasePcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_BasePcObjectTypeId",
                table: "BasePcObjects",
                column: "BasePcObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_Identifier",
                table: "BasePcObjects",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_BasePcObjects_EventTypes_PcObjectEventTypesId",
                table: "BasePcObjects_EventTypes",
                column: "PcObjectEventTypesId");

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
                name: "IX_CellResultResultEvent_ResultEventsId",
                table: "CellResultResultEvent",
                column: "ResultEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_CellResults_CeMatrixResultId",
                table: "CellResults",
                column: "CeMatrixResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CellResults_ColumnResultId",
                table: "CellResults",
                column: "ColumnResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CellResults_RowResultId",
                table: "CellResults",
                column: "RowResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_CeMatrixId",
                table: "Cells",
                column: "CeMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_ColumnId",
                table: "Cells",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Cells_RowId",
                table: "Cells",
                column: "RowId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_CeMatrixTypeId",
                table: "CeMatrices",
                column: "CeMatrixTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrices_ProjectId_Identifier",
                table: "CeMatrices",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixComments_CeMatrixId_Identifier",
                table: "CeMatrixComments",
                columns: new[] { "CeMatrixId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixDbFileReferences_CeMatrixId_Path_Name",
                table: "CeMatrixDbFileReferences",
                columns: new[] { "CeMatrixId", "Path", "Name" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixDbFileReferences_DbFileId",
                table: "CeMatrixDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixParams_CeMatrixId_ParamName",
                table: "CeMatrixParams",
                columns: new[] { "CeMatrixId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixResults_CeMatrixId",
                table: "CeMatrixResults",
                column: "CeMatrixId");

            migrationBuilder.CreateIndex(
                name: "IX_CeMatrixResults_ResultId",
                table: "CeMatrixResults",
                column: "ResultId");

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
                name: "IX_ColumnResultResultEvent_ResultEventsId",
                table: "ColumnResultResultEvent",
                column: "ResultEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnResults_CeMatrixResultId",
                table: "ColumnResults",
                column: "CeMatrixResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ColumnResults_TriggeredUnitEventId",
                table: "ColumnResults",
                column: "TriggeredUnitEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_CeMatrixId_Order",
                table: "Columns",
                columns: new[] { "CeMatrixId", "Order" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoEntities_Identifier",
                table: "CryptoEntities",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DbFiles_DbFileContentId",
                table: "DbFiles",
                column: "DbFileContentId");

            migrationBuilder.CreateIndex(
                name: "IX_DbFiles_FileBytesHash_Base64",
                table: "DbFiles",
                column: "FileBytesHash_Base64",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InformationMessages_RelatedRequestMessageId",
                table: "InformationMessages",
                column: "RelatedRequestMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Legends_ProjectId_Identifier",
                table: "Legends",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseFiles_DbFileId",
                table: "LicenseFiles",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectDbFileReferences_DbFileId",
                table: "PcObjectDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectDbFileReferences_PcObjectId",
                table: "PcObjectDbFileReferences",
                column: "PcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventDbFileReferences_DbFileId",
                table: "PcObjectEventDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventDbFileReferences_PcObjectEventId",
                table: "PcObjectEventDbFileReferences",
                column: "PcObjectEventId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectEventTypeId",
                table: "PcObjectEvents",
                column: "PcObjectEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEvents_PcObjectId",
                table: "PcObjectEvents",
                column: "PcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventTypes_IconDbFileId",
                table: "PcObjectEventTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventTypes_Type",
                table: "PcObjectEventTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectEventTypes_ParamInfos_StandardParamInfosId",
                table: "PcObjectEventTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjectJournalParams_PcObjectId",
                table: "PcObjectJournalParams",
                column: "PcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_BasePcObjectId",
                table: "PcObjects",
                column: "BasePcObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_Identifier",
                table: "PcObjects",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PcObjects_ParentId",
                table: "PcObjects",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UnitId",
                table: "Projects",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionDbFileReferences_DbFileId",
                table: "ProjectVersionDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionDbFileReferences_ProjectVersionId",
                table: "ProjectVersionDbFileReferences",
                column: "ProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersions_ProjectId_VersionNum",
                table: "ProjectVersions",
                columns: new[] { "ProjectId", "VersionNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersions_ProjectVersionTypeId",
                table: "ProjectVersions",
                column: "ProjectVersionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionTypes_IconDbFileId",
                table: "ProjectVersionTypes",
                column: "IconDbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionTypes_Type",
                table: "ProjectVersionTypes",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersionTypes_ParamInfos_StandardParamInfosId",
                table: "ProjectVersionTypes_ParamInfos",
                column: "StandardParamInfosId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultEventResultEvent_EffectResultEventsId",
                table: "ResultEventResultEvent",
                column: "EffectResultEventsId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultEventRowResult_RowResultsId",
                table: "ResultEventRowResult",
                column: "RowResultsId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultEvents_ResultId",
                table: "ResultEvents",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultEvents_TriggeredUnitEventId",
                table: "ResultEvents",
                column: "TriggeredUnitEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_UnitId",
                table: "Results",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleApiFunctionRoleBusinessFunction_RoleBusinessFunctionsId",
                table: "RoleApiFunctionRoleBusinessFunction",
                column: "RoleBusinessFunctionsId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleApiFunctions_Identifier",
                table: "RoleApiFunctions",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleBusinessFunctions_Identifier",
                table: "RoleBusinessFunctions",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleBusinessFunctionId",
                table: "RolePermissions",
                column: "RoleBusinessFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Identifier",
                table: "Roles",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RowResults_CeMatrixResultId",
                table: "RowResults",
                column: "CeMatrixResultId");

            migrationBuilder.CreateIndex(
                name: "IX_RowResults_InputTriggeredUnitEventId",
                table: "RowResults",
                column: "InputTriggeredUnitEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Rows_CeMatrixId_Order",
                table: "Rows",
                columns: new[] { "CeMatrixId", "Order" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerDbFileReferences_DbFileId",
                table: "SafetyControllerDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerDbFileReferences_SafetyControllerId_Path_Na~",
                table: "SafetyControllerDbFileReferences",
                columns: new[] { "SafetyControllerId", "Path", "Name" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllerParams_SafetyControllerId_ParamName",
                table: "SafetyControllerParams",
                columns: new[] { "SafetyControllerId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_ProjectId_Identifier",
                table: "SafetyControllers",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyControllers_SafetyControllerTypeId",
                table: "SafetyControllers",
                column: "SafetyControllerTypeId");

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
                name: "IX_TagConditionInfos_TagTypeId",
                table: "TagConditionInfos",
                column: "TagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TagConditions_TagId_ConditionCategory_AeCondition_DaConditi~",
                table: "TagConditions",
                columns: new[] { "TagId", "ConditionCategory", "AeCondition", "DaCondition" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagDbFileReferences_DbFileId",
                table: "TagDbFileReferences",
                column: "DbFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TagDbFileReferences_TagId_Path_Name",
                table: "TagDbFileReferences",
                columns: new[] { "TagId", "Path", "Name" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TagParams_TagId_ParamName",
                table: "TagParams",
                columns: new[] { "TagId", "ParamName" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_BaseActuatorId",
                table: "Tags",
                column: "BaseActuatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjectId_Identifier",
                table: "Tags",
                columns: new[] { "ProjectId", "Identifier" },
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

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

            migrationBuilder.CreateIndex(
                name: "IX_UnitEvents_UnitEventsIntervalId",
                table: "UnitEvents",
                column: "UnitEventsIntervalId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitEventsIntervals_UnitId",
                table: "UnitEventsIntervals",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_ActiveProjectVersionId",
                table: "Units",
                column: "ActiveProjectVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Identifier",
                table: "Units",
                column: "Identifier",
                unique: true,
                filter: "\"_IsDeleted\" = FALSE");

            migrationBuilder.AddForeignKey(
                name: "FK_ActuatorParams_Tags_TagId",
                table: "ActuatorParams",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuatorDbFileReferences_BaseActuators_BaseActuatorId",
                table: "BaseActuatorDbFileReferences",
                column: "BaseActuatorId",
                principalTable: "BaseActuators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuatorParams_BaseActuators_BaseActuatorId",
                table: "BaseActuatorParams",
                column: "BaseActuatorId",
                principalTable: "BaseActuators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseActuators_Projects_ProjectId",
                table: "BaseActuators",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CellResultResultEvent_CellResults_CellResultsId",
                table: "CellResultResultEvent",
                column: "CellResultsId",
                principalTable: "CellResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CellResultResultEvent_ResultEvents_ResultEventsId",
                table: "CellResultResultEvent",
                column: "ResultEventsId",
                principalTable: "ResultEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CellResults_CeMatrixResults_CeMatrixResultId",
                table: "CellResults",
                column: "CeMatrixResultId",
                principalTable: "CeMatrixResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CellResults_ColumnResults_ColumnResultId",
                table: "CellResults",
                column: "ColumnResultId",
                principalTable: "ColumnResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CellResults_RowResults_RowResultId",
                table: "CellResults",
                column: "RowResultId",
                principalTable: "RowResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cells_CeMatrices_CeMatrixId",
                table: "Cells",
                column: "CeMatrixId",
                principalTable: "CeMatrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cells_Columns_ColumnId",
                table: "Cells",
                column: "ColumnId",
                principalTable: "Columns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cells_Rows_RowId",
                table: "Cells",
                column: "RowId",
                principalTable: "Rows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrices_Projects_ProjectId",
                table: "CeMatrices",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CeMatrixResults_Results_ResultId",
                table: "CeMatrixResults",
                column: "ResultId",
                principalTable: "Results",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ColumnResultResultEvent_ColumnResults_ColumnResultsId",
                table: "ColumnResultResultEvent",
                column: "ColumnResultsId",
                principalTable: "ColumnResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ColumnResultResultEvent_ResultEvents_ResultEventsId",
                table: "ColumnResultResultEvent",
                column: "ResultEventsId",
                principalTable: "ResultEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ColumnResults_UnitEvents_TriggeredUnitEventId",
                table: "ColumnResults",
                column: "TriggeredUnitEventId",
                principalTable: "UnitEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Legends_Projects_ProjectId",
                table: "Legends",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Units_UnitId",
                table: "Projects",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectVersionTypes_DbFiles_IconDbFileId",
                table: "ProjectVersionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectVersions_Projects_ProjectId",
                table: "ProjectVersions");

            migrationBuilder.DropTable(
                name: "ActuatorParams");

            migrationBuilder.DropTable(
                name: "AddonStatuses");

            migrationBuilder.DropTable(
                name: "BaseActuatorDbFileReferences");

            migrationBuilder.DropTable(
                name: "BaseActuatorParams");

            migrationBuilder.DropTable(
                name: "BaseActuatorTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "BasePcObjectDbFileReferences");

            migrationBuilder.DropTable(
                name: "BasePcObjectJournalParams");

            migrationBuilder.DropTable(
                name: "BasePcObjects_EventTypes");

            migrationBuilder.DropTable(
                name: "BasePcObjectTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "CellResultResultEvent");

            migrationBuilder.DropTable(
                name: "Cells");

            migrationBuilder.DropTable(
                name: "CeMatrixComments");

            migrationBuilder.DropTable(
                name: "CeMatrixDbFileReferences");

            migrationBuilder.DropTable(
                name: "CeMatrixParams");

            migrationBuilder.DropTable(
                name: "CeMatrixTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "ColumnResultResultEvent");

            migrationBuilder.DropTable(
                name: "CryptoEntities");

            migrationBuilder.DropTable(
                name: "FloatJournalParamValues");

            migrationBuilder.DropTable(
                name: "InformationMessages");

            migrationBuilder.DropTable(
                name: "InformationSecurityEvents");

            migrationBuilder.DropTable(
                name: "Int32JournalParamValues");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Legends");

            migrationBuilder.DropTable(
                name: "LicenseFiles");

            migrationBuilder.DropTable(
                name: "ParamDescs");

            migrationBuilder.DropTable(
                name: "PcObjectDbFileReferences");

            migrationBuilder.DropTable(
                name: "PcObjectEventDbFileReferences");

            migrationBuilder.DropTable(
                name: "PcObjectEventTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "ProjectVersionDbFileReferences");

            migrationBuilder.DropTable(
                name: "ProjectVersionTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "ResultEventResultEvent");

            migrationBuilder.DropTable(
                name: "ResultEventRowResult");

            migrationBuilder.DropTable(
                name: "RoleApiFunctionRoleBusinessFunction");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SafetyControllerDbFileReferences");

            migrationBuilder.DropTable(
                name: "SafetyControllerParams");

            migrationBuilder.DropTable(
                name: "SafetyControllerTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "StringJournalParamValues");

            migrationBuilder.DropTable(
                name: "TagConditionInfos");

            migrationBuilder.DropTable(
                name: "TagConditions");

            migrationBuilder.DropTable(
                name: "TagDbFileReferences");

            migrationBuilder.DropTable(
                name: "TagParams");

            migrationBuilder.DropTable(
                name: "TagTypes_ParamInfos");

            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DropTable(
                name: "CellResults");

            migrationBuilder.DropTable(
                name: "Columns");

            migrationBuilder.DropTable(
                name: "Rows");

            migrationBuilder.DropTable(
                name: "RequestMessages");

            migrationBuilder.DropTable(
                name: "PcObjectEvents");

            migrationBuilder.DropTable(
                name: "ResultEvents");

            migrationBuilder.DropTable(
                name: "RoleApiFunctions");

            migrationBuilder.DropTable(
                name: "RoleBusinessFunctions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "SafetyControllers");

            migrationBuilder.DropTable(
                name: "PcObjectJournalParams");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "ParamInfos");

            migrationBuilder.DropTable(
                name: "TagTypes");

            migrationBuilder.DropTable(
                name: "ColumnResults");

            migrationBuilder.DropTable(
                name: "RowResults");

            migrationBuilder.DropTable(
                name: "PcObjectEventTypes");

            migrationBuilder.DropTable(
                name: "SafetyControllerTypes");

            migrationBuilder.DropTable(
                name: "PcObjects");

            migrationBuilder.DropTable(
                name: "BaseActuators");

            migrationBuilder.DropTable(
                name: "CeMatrixResults");

            migrationBuilder.DropTable(
                name: "UnitEvents");

            migrationBuilder.DropTable(
                name: "BasePcObjects");

            migrationBuilder.DropTable(
                name: "BaseActuatorTypes");

            migrationBuilder.DropTable(
                name: "CeMatrices");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "UnitEventsIntervals");

            migrationBuilder.DropTable(
                name: "BasePcObjectTypes");

            migrationBuilder.DropTable(
                name: "CeMatrixTypes");

            migrationBuilder.DropTable(
                name: "DbFiles");

            migrationBuilder.DropTable(
                name: "DbFileContents");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "ProjectVersions");

            migrationBuilder.DropTable(
                name: "ProjectVersionTypes");
        }
    }
}
