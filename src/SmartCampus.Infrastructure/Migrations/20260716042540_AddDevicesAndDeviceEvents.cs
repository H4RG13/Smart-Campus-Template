using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCampus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicesAndDeviceEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeviceEventId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HardwareId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthTokenHash = table.Column<string>(type: "text", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RfidTagId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceTimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceHeartbeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SignalStrength = table.Column<int>(type: "integer", nullable: false),
                    QueueDepth = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceHeartbeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceHeartbeats_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_DeviceEventId",
                table: "AttendanceRecords",
                column: "DeviceEventId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecords_OriginExclusive",
                table: "AttendanceRecords",
                sql: "(\"RecordedByUserId\" IS NOT NULL AND \"DeviceEventId\" IS NULL) OR (\"RecordedByUserId\" IS NULL AND \"DeviceEventId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId_DeviceEventId",
                table: "DeviceEvents",
                columns: new[] { "DeviceId", "DeviceEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceHeartbeats_DeviceId_ReceivedAtUtc",
                table: "DeviceHeartbeats",
                columns: new[] { "DeviceId", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_HardwareId",
                table: "Devices",
                column: "HardwareId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_DeviceEvents_DeviceEventId",
                table: "AttendanceRecords",
                column: "DeviceEventId",
                principalTable: "DeviceEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_DeviceEvents_DeviceEventId",
                table: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "DeviceEvents");

            migrationBuilder.DropTable(
                name: "DeviceHeartbeats");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_DeviceEventId",
                table: "AttendanceRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecords_OriginExclusive",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "DeviceEventId",
                table: "AttendanceRecords");
        }
    }
}
