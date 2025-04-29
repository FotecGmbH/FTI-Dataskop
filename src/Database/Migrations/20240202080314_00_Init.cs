using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class _00_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Company",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CompanyType = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Company", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Dataconverter",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CodeSnippet = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Displayname = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Dataconverter", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "File",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BlobName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PublicLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_File", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Setting",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Key = table.Column<int>(type: "int", nullable: false),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Setting", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CompanyGlobalConfig",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        GlobalConfigType = table.Column<int>(type: "int", nullable: false),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ConfigVersion = table.Column<long>(type: "bigint", nullable: false),
            //        TblCompanyId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CompanyGlobalConfig", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_CompanyGlobalConfig_Company_TblCompanyId",
            //            column: x => x.TblCompanyId,
            //            principalTable: "Company",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Gateway",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeviceCommon_FirmwareversionDevice = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceCommon_FirmwareversionService = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceCommon_ConfigversionDevice = table.Column<long>(type: "bigint", nullable: false),
            //        DeviceCommon_ConfigversionService = table.Column<long>(type: "bigint", nullable: false),
            //        DeviceCommon_State = table.Column<int>(type: "int", nullable: false),
            //        DeviceCommon_LastOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeviceCommon_LastOfflineTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeviceCommon_Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Position_Longitude = table.Column<double>(type: "float", nullable: false),
            //        Position_Latitude = table.Column<double>(type: "float", nullable: false),
            //        Position_Altitude = table.Column<double>(type: "float", nullable: false),
            //        Position_Precision = table.Column<double>(type: "float", nullable: false),
            //        Position_TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Position_Source = table.Column<int>(type: "int", nullable: false),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblCompanyId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Gateway", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Gateway_Company_TblCompanyId",
            //            column: x => x.TblCompanyId,
            //            principalTable: "Company",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Project",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Published = table.Column<bool>(type: "bit", nullable: false),
            //        PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblCompanyId = table.Column<long>(type: "bigint", nullable: false),
            //        IsPublic = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Project", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Project_Company_TblCompanyId",
            //            column: x => x.TblCompanyId,
            //            principalTable: "Company",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MeasurementDefinitionTemplate",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ValueType = table.Column<int>(type: "int", nullable: false),
            //        TblDataconverterId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MeasurementDefinitionTemplate", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_MeasurementDefinitionTemplate_Dataconverter_TblDataconverterId",
            //            column: x => x.TblDataconverterId,
            //            principalTable: "Dataconverter",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "User",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsAdmin = table.Column<bool>(type: "bit", nullable: false),
            //        AgbVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LoginName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        JwtToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Locked = table.Column<bool>(type: "bit", nullable: false),
            //        LoginConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DefaultLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ConfirmationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PushTags = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Setting10MinPush = table.Column<bool>(type: "bit", nullable: false),
            //        TblUserImageId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_User", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_User_File_TblUserImageId",
            //            column: x => x.TblUserImageId,
            //            principalTable: "File",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IotDevice",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        FallbackPosition_Longitude = table.Column<double>(type: "float", nullable: false),
            //        FallbackPosition_Latitude = table.Column<double>(type: "float", nullable: false),
            //        FallbackPosition_Altitude = table.Column<double>(type: "float", nullable: false),
            //        FallbackPosition_Precision = table.Column<double>(type: "float", nullable: false),
            //        FallbackPosition_TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        FallbackPosition_Source = table.Column<int>(type: "int", nullable: false),
            //        DeviceCommon_FirmwareversionDevice = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceCommon_FirmwareversionService = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceCommon_ConfigversionDevice = table.Column<long>(type: "bigint", nullable: false),
            //        DeviceCommon_ConfigversionService = table.Column<long>(type: "bigint", nullable: false),
            //        DeviceCommon_State = table.Column<int>(type: "int", nullable: false),
            //        DeviceCommon_LastOnlineTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeviceCommon_LastOfflineTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DeviceCommon_Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Upstream = table.Column<int>(type: "int", nullable: false),
            //        Plattform = table.Column<int>(type: "int", nullable: false),
            //        TransmissionType = table.Column<int>(type: "int", nullable: false),
            //        TransmissionInterval = table.Column<int>(type: "int", nullable: false),
            //        MeasurementInterval = table.Column<int>(type: "int", nullable: false),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SuccessfullyRegisteredInThirdPartySystem = table.Column<bool>(type: "bit", nullable: false),
            //        TblGatewayId = table.Column<long>(type: "bigint", nullable: true),
            //        TblDataconverterId = table.Column<long>(type: "bigint", nullable: true),
            //        TblCompanyGlobalConfigId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IotDevice", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IotDevice_CompanyGlobalConfig_TblCompanyGlobalConfigId",
            //            column: x => x.TblCompanyGlobalConfigId,
            //            principalTable: "CompanyGlobalConfig",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_IotDevice_Dataconverter_TblDataconverterId",
            //            column: x => x.TblDataconverterId,
            //            principalTable: "Dataconverter",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_IotDevice_Gateway_TblGatewayId",
            //            column: x => x.TblGatewayId,
            //            principalTable: "Gateway",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AccessToken",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        GuiltyUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AccessToken", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AccessToken_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Device",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DeviceHardwareId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Plattform = table.Column<int>(type: "int", nullable: false),
            //        DeviceIdiom = table.Column<int>(type: "int", nullable: false),
            //        OperatingSystemVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastDateTimeUtcOnline = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsAppRunning = table.Column<bool>(type: "bit", nullable: false),
            //        ScreenResolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Device", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Device_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Permission",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        UserRight = table.Column<int>(type: "int", nullable: false),
            //        UserRole = table.Column<int>(type: "int", nullable: false),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblCompanyId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Permission", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Permission_Company_TblCompanyId",
            //            column: x => x.TblCompanyId,
            //            principalTable: "Company",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Permission_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MeasurementDefinition",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Information_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Information_CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Information_UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        MeasurementInterval = table.Column<int>(type: "int", nullable: false),
            //        ValueType = table.Column<int>(type: "int", nullable: false),
            //        DownstreamType = table.Column<int>(type: "int", nullable: false),
            //        TblIotDeviceId = table.Column<long>(type: "bigint", nullable: false),
            //        TblLatestMeasurementResultId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MeasurementDefinition", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_MeasurementDefinition_IotDevice_TblIotDeviceId",
            //            column: x => x.TblIotDeviceId,
            //            principalTable: "IotDevice",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MeasurementDefinitionToProjectAssignment",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TblMeasurementDefinitionId = table.Column<long>(type: "bigint", nullable: true),
            //        TblProjctId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MeasurementDefinitionToProjectAssignment", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_MeasurementDefinitionToProjectAssignment_MeasurementDefinition_TblMeasurementDefinitionId",
            //            column: x => x.TblMeasurementDefinitionId,
            //            principalTable: "MeasurementDefinition",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_MeasurementDefinitionToProjectAssignment_Project_TblProjctId",
            //            column: x => x.TblProjctId,
            //            principalTable: "Project",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MeasurementResult",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Location_Longitude = table.Column<double>(type: "float", nullable: false),
            //        Location_Latitude = table.Column<double>(type: "float", nullable: false),
            //        Location_Altitude = table.Column<double>(type: "float", nullable: false),
            //        Location_Precision = table.Column<double>(type: "float", nullable: false),
            //        Location_TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Location_Source = table.Column<int>(type: "int", nullable: false),
            //        SpatialPoint = table.Column<Point>(type: "geography", nullable: false),
            //        ValueType = table.Column<int>(type: "int", nullable: false),
            //        Value_Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Value_Number = table.Column<double>(type: "float", nullable: true),
            //        Value_Binary = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
            //        Value_Bit = table.Column<bool>(type: "bit", nullable: true),
            //        AdditionalConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblMeasurementDefinitionId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MeasurementResult", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_MeasurementResult_MeasurementDefinition_TblMeasurementDefinitionId",
            //            column: x => x.TblMeasurementDefinitionId,
            //            principalTable: "MeasurementDefinition",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AccessToken_TblUserId",
            //    table: "AccessToken",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CompanyGlobalConfig_TblCompanyId",
            //    table: "CompanyGlobalConfig",
            //    column: "TblCompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Device_TblUserId",
            //    table: "Device",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Gateway_TblCompanyId",
            //    table: "Gateway",
            //    column: "TblCompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IotDevice_TblCompanyGlobalConfigId",
            //    table: "IotDevice",
            //    column: "TblCompanyGlobalConfigId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IotDevice_TblDataconverterId",
            //    table: "IotDevice",
            //    column: "TblDataconverterId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IotDevice_TblGatewayId",
            //    table: "IotDevice",
            //    column: "TblGatewayId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MeasurementDefinition_TblIotDeviceId",
            //    table: "MeasurementDefinition",
            //    column: "TblIotDeviceId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MeasurementDefinitionTemplate_TblDataconverterId",
            //    table: "MeasurementDefinitionTemplate",
            //    column: "TblDataconverterId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MeasurementDefinitionToProjectAssignment_TblMeasurementDefinitionId",
            //    table: "MeasurementDefinitionToProjectAssignment",
            //    column: "TblMeasurementDefinitionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MeasurementDefinitionToProjectAssignment_TblProjctId",
            //    table: "MeasurementDefinitionToProjectAssignment",
            //    column: "TblProjctId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MeasurementResult_TblMeasurementDefinitionId",
            //    table: "MeasurementResult",
            //    column: "TblMeasurementDefinitionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Permission_TblCompanyId",
            //    table: "Permission",
            //    column: "TblCompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Permission_TblUserId",
            //    table: "Permission",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Project_TblCompanyId",
            //    table: "Project",
            //    column: "TblCompanyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_User_TblUserImageId",
            //    table: "User",
            //    column: "TblUserImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "AccessToken");

            //migrationBuilder.DropTable(
            //    name: "Device");

            //migrationBuilder.DropTable(
            //    name: "MeasurementDefinitionTemplate");

            //migrationBuilder.DropTable(
            //    name: "MeasurementDefinitionToProjectAssignment");

            //migrationBuilder.DropTable(
            //    name: "MeasurementResult");

            //migrationBuilder.DropTable(
            //    name: "Permission");

            //migrationBuilder.DropTable(
            //    name: "Setting");

            //migrationBuilder.DropTable(
            //    name: "Project");

            //migrationBuilder.DropTable(
            //    name: "MeasurementDefinition");

            //migrationBuilder.DropTable(
            //    name: "User");

            //migrationBuilder.DropTable(
            //    name: "IotDevice");

            //migrationBuilder.DropTable(
            //    name: "File");

            //migrationBuilder.DropTable(
            //    name: "CompanyGlobalConfig");

            //migrationBuilder.DropTable(
            //    name: "Dataconverter");

            //migrationBuilder.DropTable(
            //    name: "Gateway");

            //migrationBuilder.DropTable(
            //    name: "Company");
        }
    }
}
