using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MLModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    Precision = table.Column<float>(type: "real", nullable: false),
                    Recall = table.Column<float>(type: "real", nullable: false),
                    F1Score = table.Column<float>(type: "real", nullable: false),
                    TrainingDataset = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrainingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModelFilePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TotalAnomaliesDetected = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAnomalies = table.Column<int>(type: "integer", nullable: false),
                    CriticalCount = table.Column<int>(type: "integer", nullable: false),
                    MediumCount = table.Column<int>(type: "integer", nullable: false),
                    LowCount = table.Column<int>(type: "integer", nullable: false),
                    TopThreatType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TopSourceIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    DestinationIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MLModelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_MLModels_MLModelId",
                        column: x => x.MLModelId,
                        principalTable: "MLModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BlockedIPs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IPAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedIPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedIPs_Users_BlockedBy",
                        column: x => x.BlockedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NetworkTraffics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    DestinationIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    SourcePort = table.Column<int>(type: "integer", nullable: false),
                    DestinationPort = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PacketSize = table.Column<float>(type: "real", nullable: false),
                    ConnectionsPerSecond = table.Column<float>(type: "real", nullable: false),
                    PortsContacted = table.Column<int>(type: "integer", nullable: false),
                    SessionDuration = table.Column<float>(type: "real", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlertId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkTraffics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkTraffics_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "MLModels",
                columns: new[] { "Id", "Accuracy", "Algorithm", "F1Score", "IsActive", "ModelFilePath", "Notes", "Precision", "Recall", "TotalAnomaliesDetected", "TrainingDataset", "TrainingDate", "Version" },
                values: new object[] { 1, 0f, "FastTree", 0f, true, "MLModels/model_v1.zip", "Modèle initial du projet", 0f, 0f, 0, "CICIDS2017", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "v1.0" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@detection.com", null, "CHANGE_THIS_HASH", "Admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DetectedAt",
                table: "Alerts",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_MLModelId",
                table: "Alerts",
                column: "MLModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Severity",
                table: "Alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SourceIP",
                table: "Alerts",
                column: "SourceIP");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedIPs_BlockedBy",
                table: "BlockedIPs",
                column: "BlockedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedIPs_IPAddress",
                table: "BlockedIPs",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedIPs_IsActive",
                table: "BlockedIPs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MLModels_IsActive",
                table: "MLModels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MLModels_Version",
                table: "MLModels",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkTraffics_AlertId",
                table: "NetworkTraffics",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkTraffics_CapturedAt",
                table: "NetworkTraffics",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkTraffics_SourceIP",
                table: "NetworkTraffics",
                column: "SourceIP");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GeneratedAt",
                table: "Reports",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedIPs");

            migrationBuilder.DropTable(
                name: "NetworkTraffics");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "MLModels");
        }
    }
}
