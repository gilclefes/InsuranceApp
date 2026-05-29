using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedAdjusterId",
                table: "Claims",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                table: "Claims",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionedAtUtc",
                table: "Claims",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceUrl",
                table: "Claims",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Claims",
                type: "varchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status_AssignedAdjusterId",
                table: "Claims",
                columns: new[] { "Status", "AssignedAdjusterId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_Status_AssignedAdjusterId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedAdjusterId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "EvidenceUrl",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Claims");
        }
    }
}
