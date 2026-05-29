using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductQuoteFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BaseRate = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    MinPremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDefinitions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductRiskRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    ParameterName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Operator = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRiskRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRiskRules_ProductDefinitions_ProductDefinitionId",
                        column: x => x.ProductDefinitionId,
                        principalTable: "ProductDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ProductDefinitions",
                columns: new[] { "Id", "BaseRate", "CreatedAtUtc", "CurrencyCode", "IsActive", "MinPremium", "Name", "ProductCode", "ProductType", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, 0.0100m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", true, 120.00m, "Motor Third-Party", "MOTOR-TP", 1, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, 0.0200m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", true, 50.00m, "Funeral Standard", "FUNERAL-STD", 3, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ProductRiskRules",
                columns: new[] { "Id", "AdjustmentType", "AdjustmentValue", "CreatedAtUtc", "Operator", "ParameterName", "ProductDefinitionId", "Reason", "ThresholdValue", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "Percent", 10m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "<", "ApplicantAge", 1L, "Young driver loading", 25m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "Percent", 7m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), ">", "VehicleValue", 1L, "High-value vehicle loading", 120000m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "Flat", 15m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), ">", "SumAssured", 2L, "High sum assured rider", 20000m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDefinitions_ProductCode",
                table: "ProductDefinitions",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRiskRules_ProductDefinitionId",
                table: "ProductRiskRules",
                column: "ProductDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRiskRules");

            migrationBuilder.DropTable(
                name: "ProductDefinitions");
        }
    }
}
