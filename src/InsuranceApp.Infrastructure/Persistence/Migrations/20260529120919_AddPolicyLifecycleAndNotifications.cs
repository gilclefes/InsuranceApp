using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyLifecycleAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedAgentId",
                table: "Policies",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Policies",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TemplateKey = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubjectTemplate = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyTemplate = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PolicyNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PolicyId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateKey = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Recipient = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyNotifications_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "BodyTemplate", "Channel", "CreatedAtUtc", "IsActive", "SubjectTemplate", "TemplateKey", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "Policy {PolicyNumber} is now active with premium {PremiumAmount}.", "Email", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Your policy {PolicyNumber} is active", "PolicyIssued", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "Policy {PolicyNumber} active. Premium {PremiumAmount}.", "SMS", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Policy Issued", "PolicyIssued", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "Your policy {PolicyNumber} is active.", "WhatsApp", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Policy Issued", "PolicyIssued", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "Policy {PolicyNumber} expires on {ExpiryDate}.", "Email", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Policy {PolicyNumber} expires soon", "RenewalReminder", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "Policy {PolicyNumber} expires on {ExpiryDate}.", "SMS", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Renewal Reminder", "RenewalReminder", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, "Policy {PolicyNumber} expires on {ExpiryDate}.", "WhatsApp", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Renewal Reminder", "RenewalReminder", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateKey_Channel",
                table: "NotificationTemplates",
                columns: new[] { "TemplateKey", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyNotifications_PolicyId_TemplateKey_Channel_SentAtUtc",
                table: "PolicyNotifications",
                columns: new[] { "PolicyId", "TemplateKey", "Channel", "SentAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "PolicyNotifications");

            migrationBuilder.DropColumn(
                name: "AssignedAgentId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Policies");
        }
    }
}
