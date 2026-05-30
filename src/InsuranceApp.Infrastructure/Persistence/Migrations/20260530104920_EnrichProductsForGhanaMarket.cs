using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichProductsForGhanaMarket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverageSummary",
                table: "ProductDefinitions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductDefinitions",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Exclusions",
                table: "ProductDefinitions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCoverageAmount",
                table: "ProductDefinitions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxEntryAgeYears",
                table: "ProductDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinEntryAgeYears",
                table: "ProductDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NicClassCode",
                table: "ProductDefinitions",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PolicyTermOptions",
                table: "ProductDefinitions",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UnderwritingRequirements",
                table: "ProductDefinitions",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "WaitingPeriodDays",
                table: "ProductDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CoverageSummary", "Description", "Exclusions", "MaxCoverageAmount", "MaxEntryAgeYears", "MinEntryAgeYears", "NicClassCode", "PolicyTermOptions", "UnderwritingRequirements", "WaitingPeriodDays" },
                values: new object[] { "Third-party bodily injury and death; third-party property damage up to GHS 5,000; ECOWAS Brown Card cover for cross-border travel.", "Mandatory third-party liability cover for all vehicles on Ghana roads as required by the Motor Vehicles (Third Party Insurance) Act, 1958.", "Damage to insured's own vehicle; driving under the influence of alcohol or drugs; unlicensed drivers.", 0m, 75, 18, "NIC-MOT-01", "Annual", "Valid DVLA vehicle registration; valid driver's licence; vehicle roadworthiness certificate.", 0 });

            migrationBuilder.UpdateData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CoverageSummary", "Description", "Exclusions", "MaxCoverageAmount", "MaxEntryAgeYears", "MinEntryAgeYears", "Name", "NicClassCode", "PolicyTermOptions", "UnderwritingRequirements", "WaitingPeriodDays" },
                values: new object[] { "Lump-sum payout on death; covers coffin, mortuary fees, transportation of remains, and customary rites.", "Affordable funeral expense cover paying a lump sum upon death of the insured to assist families with funeral and burial costs in line with Ghanaian traditions.", "Suicide within 12 months; death from pre-existing condition not disclosed at enrollment.", 50000.00m, 70, 18, "Funeral Policy (Standard)", "NIC-LIF-04", "Monthly,Annual", "Ghana Card or valid national ID; completed health declaration form.", 180 });

            migrationBuilder.InsertData(
                table: "ProductDefinitions",
                columns: new[] { "Id", "BaseRate", "CoverageSummary", "CreatedAtUtc", "CurrencyCode", "Description", "Exclusions", "IsActive", "MaxCoverageAmount", "MaxEntryAgeYears", "MinEntryAgeYears", "MinPremium", "Name", "NicClassCode", "PolicyTermOptions", "ProductCode", "ProductType", "UnderwritingRequirements", "UpdatedAtUtc", "WaitingPeriodDays" },
                values: new object[,]
                {
                    { 3L, 0.0350m, "Own damage and total loss; fire and theft; third-party bodily injury and property damage; towing and recovery; personal accident for driver and passengers.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Full cover for private and commercial vehicles including own damage, theft, fire, and third-party liability.", "Mechanical and electrical breakdown; wear and tear; driving without valid licence; racing or speed testing.", true, 500000.00m, 75, 18, 450.00m, "Motor Comprehensive", "NIC-MOT-02", "Annual", "MOTOR-COMP", 1, "Vehicle inspection report; valid DVLA registration; driver's licence; proof of vehicle value (invoice or valuation).", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 4L, 0.0200m, "Third-party bodily injury and property damage; fire damage to insured vehicle; vehicle theft and attempted theft.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Intermediate motor cover combining mandatory third-party liability with fire damage and theft protection.", "Own damage from accidents; mechanical breakdown; consequential loss.", true, 300000.00m, 75, 18, 250.00m, "Motor Third-Party Fire & Theft", "NIC-MOT-03", "Annual", "MOTOR-TPF", 1, "Valid DVLA registration; driver's licence; vehicle roadworthiness certificate.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 5L, 0.0150m, "Guaranteed maturity benefit for school fees; death benefit equal to sum assured; premium waiver on death or total permanent disability of parent.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "A savings-linked insurance plan ensuring children's education fees are covered even if the parent or guardian passes away or becomes permanently disabled.", "Suicide within 12 months; fraudulent claims.", true, 200000.00m, 55, 18, 100.00m, "Education Endowment Plan", "NIC-LIF-02", "5 Years,10 Years,15 Years", "EDU-PLAN", 2, "Ghana Card; child's birth certificate; health declaration form; proof of income.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 6L, 0.0080m, "Death benefit payable to named beneficiaries; optional accidental death double indemnity; conversion option to whole life.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Pure protection cover paying a lump sum to beneficiaries if the policyholder dies within the selected term.", "Suicide within 24 months; death from war or terrorism; hazardous occupations not disclosed.", true, 1000000.00m, 65, 18, 80.00m, "Term Life Assurance", "NIC-LIF-01", "5 Years,10 Years,15 Years,20 Years", "LIFE-TERM", 5, "Ghana Card; medical examination for sum assured above GHS 100,000; completed proposal form.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 7L, 0.0400m, "In-patient and out-patient treatment; specialist and diagnostic services; prescribed medicines; emergency evacuation within Ghana; maternity cover (optional).", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Complementary health cover providing access to private hospitals, specialist consultations, and prescription medicines beyond NHIS coverage.", "Pre-existing conditions (first 12 months); cosmetic surgery; self-inflicted injuries; HIV/AIDS treatment (unless rider purchased).", true, 100000.00m, 65, 0, 200.00m, "Private Health Insurance", "NIC-HLT-01", "Annual", "HEALTH-PRIV", 6, "Ghana Card or birth certificate (dependants); NHIS registration number; health questionnaire.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 30 },
                    { 8L, 0.0025m, "Building structure; contents and personal belongings; liability to domestic employees; temporary alternative accommodation; burst pipes and water damage.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Protection for residential properties against fire, flood, storm, theft, and other perils common in Ghana.", "War and civil commotion; gradual deterioration; illegal structures; unoccupied property beyond 30 days without notice.", true, 2000000.00m, 99, 18, 150.00m, "Home & Property Insurance", "NIC-FIR-02", "Annual", "HOME-PROP", 11, "Property valuation report; proof of ownership or tenancy; Ghana Card.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 9L, 0.0500m, "Emergency medical treatment abroad; medical evacuation and repatriation; trip cancellation and curtailment; lost or delayed baggage; personal liability.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Covers Ghanaian travellers for medical emergencies, trip cancellation, lost luggage, and personal liability while abroad.", "Travel against medical advice; extreme sports (unless rider); pre-existing conditions; travel to sanctioned countries.", true, 500000.00m, 80, 0, 50.00m, "Travel Insurance", "NIC-MIS-01", "Single Trip,Annual Multi-Trip", "TRAVEL-GH", 9, "Valid passport; flight itinerary; Ghana Card.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 10L, 0.0060m, "Accidental death benefit; permanent total disability; permanent partial disability (schedule of benefits); temporary total disability weekly benefit; medical expenses from accident.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Pays defined benefits for accidental death, permanent disability, or temporary disability arising from accidents.", "Self-inflicted injuries; injuries while under influence of drugs/alcohol; injuries from criminal activity; war and terrorism.", true, 200000.00m, 65, 18, 60.00m, "Personal Accident", "NIC-ACC-01", "Annual", "PA-COVER", 10, "Ghana Card; occupation declaration; health declaration for high-risk occupations.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 11L, 0.0030m, "Fire and lightning; explosion; storm and flood; aircraft damage; riot and strikes; impact by vehicles; burst pipes; business interruption (optional).", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Covers commercial properties including shops, warehouses, and offices against fire, lightning, explosion, and allied perils.", "Arson by insured; war and nuclear risks; gradual deterioration; electrical/mechanical breakdown.", true, 10000000.00m, 99, 0, 300.00m, "Fire & Allied Perils (Commercial)", "NIC-FIR-01", "Annual", "FIRE-COM", 7, "Property valuation; fire safety inspection report; business registration certificate.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 12L, 0.0045m, "Loss or damage to cargo during transit; general average contribution; salvage charges; warehouse-to-warehouse cover.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Protects goods in transit by sea, air, or land, covering imports through Tema and Takoradi ports.", "Inherent vice of goods; willful misconduct; delay; ordinary leakage and breakage; nuclear risks.", true, 5000000.00m, 99, 0, 500.00m, "Marine Cargo Insurance", "NIC-MAR-01", "Per Shipment,Annual Open Cover", "MARINE-CARGO", 8, "Commercial invoice; bill of lading or airway bill; packing list; import declaration form (IDF).", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 13L, 0.0500m, "Crop loss from drought, flood, pest, and disease; replanting costs; yield shortfall indemnity; parametric (weather-index) trigger option.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Index-based and multi-peril crop cover designed for Ghana's cocoa, maize, rice, and vegetable farmers under the Ghana Agricultural Insurance Programme (GAIP) framework.", "Losses due to negligent farming practices; theft of harvest; government-mandated destruction.", true, 100000.00m, 75, 18, 30.00m, "Agricultural Crop Insurance", "NIC-AGR-01", "Seasonal,Annual", "AGRI-CROP", 12, "Farm location GPS coordinates; crop type and acreage declaration; farmer ID or Ghana Card.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { 14L, 0.0300m, "Death benefit; hospitalisation daily cash benefit; permanent disability benefit. Premiums collectible via MTN MoMo, Vodafone Cash, or AirtelTigo Money.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "GHS", "Affordable, NIC-regulated micro-insurance for low-income earners offering basic life and hospital cash benefits, distributed via mobile money.", "Suicide; death from pre-existing illness within 6 months; injuries from criminal activity.", true, 10000.00m, 60, 18, 5.00m, "Micro-Insurance (Nhyira Plan)", "NIC-MIC-01", "Monthly,Quarterly,Annual", "MICRO-INS", 13, "Ghana Card or voter ID; mobile money wallet number; basic health declaration.", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 30 }
                });

            migrationBuilder.InsertData(
                table: "ProductRiders",
                columns: new[] { "Id", "AdjustmentType", "AdjustmentValue", "CreatedAtUtc", "IsActive", "Name", "ProductDefinitionId", "RiderCode", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 3L, "Flat", 50m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Excess Buy-Back", 3L, "EXCESS_BUY", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, "Flat", 35m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "24hr Roadside Assistance & Towing", 3L, "TOWING", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "Percent", 3m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Accidental Death Double Indemnity", 6L, "DBL_INDEM", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, "Flat", 100m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Maternity Benefit", 7L, "MATERNITY", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, "Flat", 60m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Dental & Optical Cover", 7L, "DENTAL", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8L, "Percent", 15m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), true, "Extreme Sports Cover", 9L, "EXTREME_SP", new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ProductRiskRules",
                columns: new[] { "Id", "AdjustmentType", "AdjustmentValue", "CreatedAtUtc", "Operator", "ParameterName", "ProductDefinitionId", "Reason", "ThresholdValue", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 4L, "Percent", 15m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), "<", "ApplicantAge", 3L, "Young driver loading (comprehensive)", 25m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, "Percent", 5m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), ">", "VehicleValue", 3L, "High-value vehicle loading", 200000m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, "Percent", 20m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), ">", "ApplicantAge", 6L, "Older age life loading", 50m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, "Flat", 50m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), ">", "SumAssured", 6L, "Medical examination required surcharge", 100000m, new DateTime(2026, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ProductRiders",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ProductRiskRules",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "ProductRiskRules",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ProductRiskRules",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ProductRiskRules",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DropColumn(
                name: "CoverageSummary",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "Exclusions",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "MaxCoverageAmount",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "MaxEntryAgeYears",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "MinEntryAgeYears",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "NicClassCode",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "PolicyTermOptions",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "UnderwritingRequirements",
                table: "ProductDefinitions");

            migrationBuilder.DropColumn(
                name: "WaitingPeriodDays",
                table: "ProductDefinitions");

            migrationBuilder.UpdateData(
                table: "ProductDefinitions",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Name",
                value: "Funeral Standard");
        }
    }
}
