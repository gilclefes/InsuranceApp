using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductRidersAndQuoteRiderSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedRiderCodes",
                table: "QuoteRecords",
                type: "varchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductRiders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    RiderCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdjustmentType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRiders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRiders_ProductDefinitions_ProductDefinitionId",
                        column: x => x.ProductDefinitionId,
                        principalTable: "ProductDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ProductRiders",
                columns: new[] { "Id", "AdjustmentType", "AdjustmentValue", "CreatedAtUtc", "IsActive", "Name", "ProductDefinitionId", "RiderCode", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1L, "Flat", 20m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Windscreen Cover", 1L, "WINDSHIELD", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "Percent", 5m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Family Plus Rider", 2L, "FAMILY_PLUS", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRiders_ProductDefinitionId_RiderCode",
                table: "ProductRiders",
                columns: new[] { "ProductDefinitionId", "RiderCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRiders");

            migrationBuilder.DropColumn(
                name: "SelectedRiderCodes",
                table: "QuoteRecords");
        }
    }
}
