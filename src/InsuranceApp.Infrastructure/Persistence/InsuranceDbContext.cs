using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

public class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyNotification> PolicyNotifications => Set<PolicyNotification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<PolicyDocument> PolicyDocuments => Set<PolicyDocument>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<PremiumTransaction> PremiumTransactions => Set<PremiumTransaction>();
    public DbSet<PremiumCollectionWebhookLog> PremiumCollectionWebhookLogs => Set<PremiumCollectionWebhookLog>();
    public DbSet<ClaimTimelineEvent> ClaimTimelineEvents => Set<ClaimTimelineEvent>();
    public DbSet<ClaimNotification> ClaimNotifications => Set<ClaimNotification>();
    public DbSet<PayoutTransaction> PayoutTransactions => Set<PayoutTransaction>();
    public DbSet<PayoutWebhookLog> PayoutWebhookLogs => Set<PayoutWebhookLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();
    public DbSet<IdentityAuditLog> IdentityAuditLogs => Set<IdentityAuditLog>();
    public DbSet<QuoteRecord> QuoteRecords => Set<QuoteRecord>();
    public DbSet<ProductDefinition> ProductDefinitions => Set<ProductDefinition>();
    public DbSet<ProductRiskRule> ProductRiskRules => Set<ProductRiskRule>();
    public DbSet<ProductRider> ProductRiders => Set<ProductRider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.CustomerNumber).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.CustomerNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.GhanaCardNumberHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.RegisteredByAgentId).HasMaxLength(128);
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasIndex(x => x.PolicyNumber).IsUnique();
            entity.Property(x => x.PolicyNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssignedAgentId).HasMaxLength(128);
            entity.Property(x => x.CoverageType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.CancellationReason).HasMaxLength(256);
            entity.Property(x => x.PremiumAmount).HasPrecision(18, 2);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Policies)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PolicyNotification>(entity =>
        {
            entity.Property(x => x.TemplateKey).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Channel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Recipient).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => new { x.PolicyId, x.TemplateKey, x.Channel, x.SentAtUtc });

            entity.HasOne(x => x.Policy)
                .WithMany()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasIndex(x => new { x.TemplateKey, x.Channel }).IsUnique();
            entity.Property(x => x.TemplateKey).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Channel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SubjectTemplate).HasMaxLength(200).IsRequired();
            entity.Property(x => x.BodyTemplate).HasMaxLength(2000).IsRequired();

            entity.HasData(
                new NotificationTemplate
                {
                    Id = 1,
                    TemplateKey = "PolicyIssued",
                    Channel = "Email",
                    SubjectTemplate = "Your policy {PolicyNumber} is active",
                    BodyTemplate = "Policy {PolicyNumber} is now active with premium {PremiumAmount}.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new NotificationTemplate
                {
                    Id = 2,
                    TemplateKey = "PolicyIssued",
                    Channel = "SMS",
                    SubjectTemplate = "Policy Issued",
                    BodyTemplate = "Policy {PolicyNumber} active. Premium {PremiumAmount}.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new NotificationTemplate
                {
                    Id = 3,
                    TemplateKey = "PolicyIssued",
                    Channel = "WhatsApp",
                    SubjectTemplate = "Policy Issued",
                    BodyTemplate = "Your policy {PolicyNumber} is active.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new NotificationTemplate
                {
                    Id = 4,
                    TemplateKey = "RenewalReminder",
                    Channel = "Email",
                    SubjectTemplate = "Policy {PolicyNumber} expires soon",
                    BodyTemplate = "Policy {PolicyNumber} expires on {ExpiryDate}.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new NotificationTemplate
                {
                    Id = 5,
                    TemplateKey = "RenewalReminder",
                    Channel = "SMS",
                    SubjectTemplate = "Renewal Reminder",
                    BodyTemplate = "Policy {PolicyNumber} expires on {ExpiryDate}.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new NotificationTemplate
                {
                    Id = 6,
                    TemplateKey = "RenewalReminder",
                    Channel = "WhatsApp",
                    SubjectTemplate = "Renewal Reminder",
                    BodyTemplate = "Policy {PolicyNumber} expires on {ExpiryDate}.",
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                });
        });

            modelBuilder.Entity<PolicyDocument>(entity =>
            {
                entity.HasIndex(x => x.DocumentReference).IsUnique();
                entity.Property(x => x.DocumentReference).HasMaxLength(64).IsRequired();
                entity.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.FileName).HasMaxLength(128).IsRequired();
                entity.Property(x => x.ContentType).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Content).HasColumnType("longblob");

                entity.HasOne(x => x.Policy)
                .WithMany()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasIndex(x => x.ClaimNumber).IsUnique();
            entity.HasIndex(x => new { x.Status, x.AssignedAdjusterId });
            entity.Property(x => x.ClaimNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ClaimType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.EvidenceUrl).HasMaxLength(256);
            entity.Property(x => x.AssignedAdjusterId).HasMaxLength(128);
            entity.Property(x => x.DecisionReason).HasMaxLength(256);
            entity.Property(x => x.ReviewNotes).HasMaxLength(512);
            entity.Property(x => x.FraudScore).HasPrecision(5, 2);
            entity.Property(x => x.FraudReason).HasMaxLength(256);
            entity.Property(x => x.ClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);

            entity.HasOne(x => x.Policy)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerDocument>(entity =>
        {
            entity.HasIndex(x => x.DocumentReference).IsUnique();
            entity.HasIndex(x => x.CustomerId);
            entity.Property(x => x.DocumentReference).HasMaxLength(96).IsRequired();
            entity.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(32).IsRequired();
            entity.Property(x => x.StorageUrl).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.UploadedByUserId).HasMaxLength(128);

            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Policy)
                .WithMany()
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Claim)
                .WithMany()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.SetNull);
        });

            modelBuilder.Entity<ClaimTimelineEvent>(entity =>
            {
                entity.Property(x => x.EventType).HasMaxLength(40).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
                entity.Property(x => x.ActorUserId).HasMaxLength(128);
                entity.HasIndex(x => new { x.ClaimId, x.EventAtUtc });

                entity.HasOne(x => x.Claim)
                .WithMany()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClaimNotification>(entity =>
            {
                entity.Property(x => x.Channel).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Recipient).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
                entity.HasIndex(x => new { x.ClaimId, x.SentAtUtc });

                entity.HasOne(x => x.Claim)
                .WithMany()
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<PremiumTransaction>(entity =>
        {
            entity.HasIndex(x => x.TransactionReference).IsUnique();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.Property(x => x.TransactionReference).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128);
            entity.Property(x => x.Provider).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProviderReference).HasMaxLength(128);
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PaymentChannel).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);

            entity.HasOne(x => x.Policy)
                .WithMany(x => x.PremiumTransactions)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

            modelBuilder.Entity<PremiumCollectionWebhookLog>(entity =>
            {
                entity.HasIndex(x => x.EventId).IsUnique();
                entity.Property(x => x.Provider).HasMaxLength(64).IsRequired();
                entity.Property(x => x.EventId).HasMaxLength(128).IsRequired();
                entity.Property(x => x.TransactionReference).HasMaxLength(128).IsRequired();
                entity.Property(x => x.PolicyNumber).HasMaxLength(64).IsRequired();
                entity.Property(x => x.Payload).HasColumnType("longtext");
                entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            });

        modelBuilder.Entity<PayoutTransaction>(entity =>
        {
            entity.HasIndex(x => x.PayoutReference).IsUnique();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.Property(x => x.PayoutReference).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128);
            entity.Property(x => x.ProviderReference).HasMaxLength(128);
            entity.Property(x => x.DestinationChannel).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DestinationAccount).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.Amount).HasPrecision(18, 2);

            entity.HasOne(x => x.Claim)
                .WithMany(x => x.PayoutTransactions)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
        });

            modelBuilder.Entity<PayoutWebhookLog>(entity =>
            {
                entity.HasIndex(x => x.EventId).IsUnique();
                entity.Property(x => x.Provider).HasMaxLength(64).IsRequired();
                entity.Property(x => x.EventId).HasMaxLength(128).IsRequired();
                entity.Property(x => x.PayoutReference).HasMaxLength(128).IsRequired();
                entity.Property(x => x.Payload).HasColumnType("longtext");
                entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(x => x.Token).IsUnique();
            entity.Property(x => x.Token).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SessionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DeviceId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.Property(x => x.ReplacedByToken).HasMaxLength(256);
            entity.Property(x => x.RevokedReason).HasMaxLength(128);
            entity.HasIndex(x => new { x.UserId, x.SessionId });
        });

        modelBuilder.Entity<OtpChallenge>(entity =>
        {
            entity.HasIndex(x => x.ChallengeId).IsUnique();
            entity.Property(x => x.ChallengeId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Channel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);
            entity.HasIndex(x => new { x.Destination, x.Purpose, x.CreatedAtUtc });
        });

        modelBuilder.Entity<IdentityAuditLog>(entity =>
        {
            entity.Property(x => x.Action).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SubjectId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UserAgent).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => new { x.Action, x.CreatedAtUtc });
        });

        modelBuilder.Entity<QuoteRecord>(entity =>
        {
            entity.HasIndex(x => x.QuoteReference).IsUnique();
            entity.Property(x => x.QuoteReference).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProductCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BasePremium).HasPrecision(18, 2);
            entity.Property(x => x.TotalPremium).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.SelectedRiderCodes).HasMaxLength(400);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.IssuedPolicyNumber).HasMaxLength(64);
        });

        modelBuilder.Entity<ProductDefinition>(entity =>
        {
            entity.HasIndex(x => x.ProductCode).IsUnique();
            entity.Property(x => x.ProductCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CoverageSummary).HasMaxLength(1000);
            entity.Property(x => x.NicClassCode).HasMaxLength(16);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.BaseRate).HasPrecision(10, 4);
            entity.Property(x => x.MinPremium).HasPrecision(18, 2);
            entity.Property(x => x.MaxCoverageAmount).HasPrecision(18, 2);
            entity.Property(x => x.PolicyTermOptions).HasMaxLength(120);
            entity.Property(x => x.Exclusions).HasMaxLength(1000);
            entity.Property(x => x.UnderwritingRequirements).HasMaxLength(1000);

            var seedDate = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                new ProductDefinition
                {
                    Id = 1,
                    ProductCode = "MOTOR-TP",
                    Name = "Motor Third-Party",
                    Description = "Mandatory third-party liability cover for all vehicles on Ghana roads as required by the Motor Vehicles (Third Party Insurance) Act, 1958.",
                    CoverageSummary = "Third-party bodily injury and death; third-party property damage up to GHS 5,000; ECOWAS Brown Card cover for cross-border travel.",
                    ProductType = Domain.Enums.ProductType.Motor,
                    NicClassCode = "NIC-MOT-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0100m,
                    MinPremium = 120.00m,
                    MaxCoverageAmount = 0m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 75,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Damage to insured's own vehicle; driving under the influence of alcohol or drugs; unlicensed drivers.",
                    UnderwritingRequirements = "Valid DVLA vehicle registration; valid driver's licence; vehicle roadworthiness certificate.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 2,
                    ProductCode = "FUNERAL-STD",
                    Name = "Funeral Policy (Standard)",
                    Description = "Affordable funeral expense cover paying a lump sum upon death of the insured to assist families with funeral and burial costs in line with Ghanaian traditions.",
                    CoverageSummary = "Lump-sum payout on death; covers coffin, mortuary fees, transportation of remains, and customary rites.",
                    ProductType = Domain.Enums.ProductType.Funeral,
                    NicClassCode = "NIC-LIF-04",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0200m,
                    MinPremium = 50.00m,
                    MaxCoverageAmount = 50000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 70,
                    PolicyTermOptions = "Monthly,Annual",
                    WaitingPeriodDays = 180,
                    Exclusions = "Suicide within 12 months; death from pre-existing condition not disclosed at enrollment.",
                    UnderwritingRequirements = "Ghana Card or valid national ID; completed health declaration form.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 3,
                    ProductCode = "MOTOR-COMP",
                    Name = "Motor Comprehensive",
                    Description = "Full cover for private and commercial vehicles including own damage, theft, fire, and third-party liability.",
                    CoverageSummary = "Own damage and total loss; fire and theft; third-party bodily injury and property damage; towing and recovery; personal accident for driver and passengers.",
                    ProductType = Domain.Enums.ProductType.Motor,
                    NicClassCode = "NIC-MOT-02",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0350m,
                    MinPremium = 450.00m,
                    MaxCoverageAmount = 500000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 75,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Mechanical and electrical breakdown; wear and tear; driving without valid licence; racing or speed testing.",
                    UnderwritingRequirements = "Vehicle inspection report; valid DVLA registration; driver's licence; proof of vehicle value (invoice or valuation).",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 4,
                    ProductCode = "MOTOR-TPF",
                    Name = "Motor Third-Party Fire & Theft",
                    Description = "Intermediate motor cover combining mandatory third-party liability with fire damage and theft protection.",
                    CoverageSummary = "Third-party bodily injury and property damage; fire damage to insured vehicle; vehicle theft and attempted theft.",
                    ProductType = Domain.Enums.ProductType.Motor,
                    NicClassCode = "NIC-MOT-03",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0200m,
                    MinPremium = 250.00m,
                    MaxCoverageAmount = 300000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 75,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Own damage from accidents; mechanical breakdown; consequential loss.",
                    UnderwritingRequirements = "Valid DVLA registration; driver's licence; vehicle roadworthiness certificate.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 5,
                    ProductCode = "EDU-PLAN",
                    Name = "Education Endowment Plan",
                    Description = "A savings-linked insurance plan ensuring children's education fees are covered even if the parent or guardian passes away or becomes permanently disabled.",
                    CoverageSummary = "Guaranteed maturity benefit for school fees; death benefit equal to sum assured; premium waiver on death or total permanent disability of parent.",
                    ProductType = Domain.Enums.ProductType.Education,
                    NicClassCode = "NIC-LIF-02",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0150m,
                    MinPremium = 100.00m,
                    MaxCoverageAmount = 200000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 55,
                    PolicyTermOptions = "5 Years,10 Years,15 Years",
                    WaitingPeriodDays = 0,
                    Exclusions = "Suicide within 12 months; fraudulent claims.",
                    UnderwritingRequirements = "Ghana Card; child's birth certificate; health declaration form; proof of income.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 6,
                    ProductCode = "LIFE-TERM",
                    Name = "Term Life Assurance",
                    Description = "Pure protection cover paying a lump sum to beneficiaries if the policyholder dies within the selected term.",
                    CoverageSummary = "Death benefit payable to named beneficiaries; optional accidental death double indemnity; conversion option to whole life.",
                    ProductType = Domain.Enums.ProductType.Life,
                    NicClassCode = "NIC-LIF-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0080m,
                    MinPremium = 80.00m,
                    MaxCoverageAmount = 1000000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 65,
                    PolicyTermOptions = "5 Years,10 Years,15 Years,20 Years",
                    WaitingPeriodDays = 0,
                    Exclusions = "Suicide within 24 months; death from war or terrorism; hazardous occupations not disclosed.",
                    UnderwritingRequirements = "Ghana Card; medical examination for sum assured above GHS 100,000; completed proposal form.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 7,
                    ProductCode = "HEALTH-PRIV",
                    Name = "Private Health Insurance",
                    Description = "Complementary health cover providing access to private hospitals, specialist consultations, and prescription medicines beyond NHIS coverage.",
                    CoverageSummary = "In-patient and out-patient treatment; specialist and diagnostic services; prescribed medicines; emergency evacuation within Ghana; maternity cover (optional).",
                    ProductType = Domain.Enums.ProductType.Health,
                    NicClassCode = "NIC-HLT-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0400m,
                    MinPremium = 200.00m,
                    MaxCoverageAmount = 100000.00m,
                    MinEntryAgeYears = 0,
                    MaxEntryAgeYears = 65,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 30,
                    Exclusions = "Pre-existing conditions (first 12 months); cosmetic surgery; self-inflicted injuries; HIV/AIDS treatment (unless rider purchased).",
                    UnderwritingRequirements = "Ghana Card or birth certificate (dependants); NHIS registration number; health questionnaire.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 8,
                    ProductCode = "HOME-PROP",
                    Name = "Home & Property Insurance",
                    Description = "Protection for residential properties against fire, flood, storm, theft, and other perils common in Ghana.",
                    CoverageSummary = "Building structure; contents and personal belongings; liability to domestic employees; temporary alternative accommodation; burst pipes and water damage.",
                    ProductType = Domain.Enums.ProductType.Home,
                    NicClassCode = "NIC-FIR-02",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0025m,
                    MinPremium = 150.00m,
                    MaxCoverageAmount = 2000000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 99,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "War and civil commotion; gradual deterioration; illegal structures; unoccupied property beyond 30 days without notice.",
                    UnderwritingRequirements = "Property valuation report; proof of ownership or tenancy; Ghana Card.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 9,
                    ProductCode = "TRAVEL-GH",
                    Name = "Travel Insurance",
                    Description = "Covers Ghanaian travellers for medical emergencies, trip cancellation, lost luggage, and personal liability while abroad.",
                    CoverageSummary = "Emergency medical treatment abroad; medical evacuation and repatriation; trip cancellation and curtailment; lost or delayed baggage; personal liability.",
                    ProductType = Domain.Enums.ProductType.Travel,
                    NicClassCode = "NIC-MIS-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0500m,
                    MinPremium = 50.00m,
                    MaxCoverageAmount = 500000.00m,
                    MinEntryAgeYears = 0,
                    MaxEntryAgeYears = 80,
                    PolicyTermOptions = "Single Trip,Annual Multi-Trip",
                    WaitingPeriodDays = 0,
                    Exclusions = "Travel against medical advice; extreme sports (unless rider); pre-existing conditions; travel to sanctioned countries.",
                    UnderwritingRequirements = "Valid passport; flight itinerary; Ghana Card.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 10,
                    ProductCode = "PA-COVER",
                    Name = "Personal Accident",
                    Description = "Pays defined benefits for accidental death, permanent disability, or temporary disability arising from accidents.",
                    CoverageSummary = "Accidental death benefit; permanent total disability; permanent partial disability (schedule of benefits); temporary total disability weekly benefit; medical expenses from accident.",
                    ProductType = Domain.Enums.ProductType.PersonalAccident,
                    NicClassCode = "NIC-ACC-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0060m,
                    MinPremium = 60.00m,
                    MaxCoverageAmount = 200000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 65,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Self-inflicted injuries; injuries while under influence of drugs/alcohol; injuries from criminal activity; war and terrorism.",
                    UnderwritingRequirements = "Ghana Card; occupation declaration; health declaration for high-risk occupations.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 11,
                    ProductCode = "FIRE-COM",
                    Name = "Fire & Allied Perils (Commercial)",
                    Description = "Covers commercial properties including shops, warehouses, and offices against fire, lightning, explosion, and allied perils.",
                    CoverageSummary = "Fire and lightning; explosion; storm and flood; aircraft damage; riot and strikes; impact by vehicles; burst pipes; business interruption (optional).",
                    ProductType = Domain.Enums.ProductType.Fire,
                    NicClassCode = "NIC-FIR-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0030m,
                    MinPremium = 300.00m,
                    MaxCoverageAmount = 10000000.00m,
                    MinEntryAgeYears = 0,
                    MaxEntryAgeYears = 99,
                    PolicyTermOptions = "Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Arson by insured; war and nuclear risks; gradual deterioration; electrical/mechanical breakdown.",
                    UnderwritingRequirements = "Property valuation; fire safety inspection report; business registration certificate.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 12,
                    ProductCode = "MARINE-CARGO",
                    Name = "Marine Cargo Insurance",
                    Description = "Protects goods in transit by sea, air, or land, covering imports through Tema and Takoradi ports.",
                    CoverageSummary = "Loss or damage to cargo during transit; general average contribution; salvage charges; warehouse-to-warehouse cover.",
                    ProductType = Domain.Enums.ProductType.Marine,
                    NicClassCode = "NIC-MAR-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0045m,
                    MinPremium = 500.00m,
                    MaxCoverageAmount = 5000000.00m,
                    MinEntryAgeYears = 0,
                    MaxEntryAgeYears = 99,
                    PolicyTermOptions = "Per Shipment,Annual Open Cover",
                    WaitingPeriodDays = 0,
                    Exclusions = "Inherent vice of goods; willful misconduct; delay; ordinary leakage and breakage; nuclear risks.",
                    UnderwritingRequirements = "Commercial invoice; bill of lading or airway bill; packing list; import declaration form (IDF).",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 13,
                    ProductCode = "AGRI-CROP",
                    Name = "Agricultural Crop Insurance",
                    Description = "Index-based and multi-peril crop cover designed for Ghana's cocoa, maize, rice, and vegetable farmers under the Ghana Agricultural Insurance Programme (GAIP) framework.",
                    CoverageSummary = "Crop loss from drought, flood, pest, and disease; replanting costs; yield shortfall indemnity; parametric (weather-index) trigger option.",
                    ProductType = Domain.Enums.ProductType.Agricultural,
                    NicClassCode = "NIC-AGR-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0500m,
                    MinPremium = 30.00m,
                    MaxCoverageAmount = 100000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 75,
                    PolicyTermOptions = "Seasonal,Annual",
                    WaitingPeriodDays = 0,
                    Exclusions = "Losses due to negligent farming practices; theft of harvest; government-mandated destruction.",
                    UnderwritingRequirements = "Farm location GPS coordinates; crop type and acreage declaration; farmer ID or Ghana Card.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                },
                new ProductDefinition
                {
                    Id = 14,
                    ProductCode = "MICRO-INS",
                    Name = "Micro-Insurance (Nhyira Plan)",
                    Description = "Affordable, NIC-regulated micro-insurance for low-income earners offering basic life and hospital cash benefits, distributed via mobile money.",
                    CoverageSummary = "Death benefit; hospitalisation daily cash benefit; permanent disability benefit. Premiums collectible via MTN MoMo, Vodafone Cash, or AirtelTigo Money.",
                    ProductType = Domain.Enums.ProductType.MicroInsurance,
                    NicClassCode = "NIC-MIC-01",
                    CurrencyCode = "GHS",
                    BaseRate = 0.0300m,
                    MinPremium = 5.00m,
                    MaxCoverageAmount = 10000.00m,
                    MinEntryAgeYears = 18,
                    MaxEntryAgeYears = 60,
                    PolicyTermOptions = "Monthly,Quarterly,Annual",
                    WaitingPeriodDays = 30,
                    Exclusions = "Suicide; death from pre-existing illness within 6 months; injuries from criminal activity.",
                    UnderwritingRequirements = "Ghana Card or voter ID; mobile money wallet number; basic health declaration.",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                });
        });

        modelBuilder.Entity<ProductRiskRule>(entity =>
        {
            entity.Property(x => x.ParameterName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Operator).HasMaxLength(8).IsRequired();
            entity.Property(x => x.AdjustmentType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ThresholdValue).HasPrecision(18, 2);
            entity.Property(x => x.AdjustmentValue).HasPrecision(18, 2);

            entity.HasOne(x => x.ProductDefinition)
                .WithMany(x => x.RiskRules)
                .HasForeignKey(x => x.ProductDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new ProductRiskRule
                {
                    Id = 1,
                    ProductDefinitionId = 1,
                    ParameterName = "ApplicantAge",
                    Operator = "<",
                    ThresholdValue = 25,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 10,
                    Reason = "Young driver loading",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 2,
                    ProductDefinitionId = 1,
                    ParameterName = "VehicleValue",
                    Operator = ">",
                    ThresholdValue = 120000,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 7,
                    Reason = "High-value vehicle loading",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 3,
                    ProductDefinitionId = 2,
                    ParameterName = "SumAssured",
                    Operator = ">",
                    ThresholdValue = 20000,
                    AdjustmentType = "Flat",
                    AdjustmentValue = 15,
                    Reason = "High sum assured rider",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 4,
                    ProductDefinitionId = 3,
                    ParameterName = "ApplicantAge",
                    Operator = "<",
                    ThresholdValue = 25,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 15,
                    Reason = "Young driver loading (comprehensive)",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 5,
                    ProductDefinitionId = 3,
                    ParameterName = "VehicleValue",
                    Operator = ">",
                    ThresholdValue = 200000,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 5,
                    Reason = "High-value vehicle loading",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 6,
                    ProductDefinitionId = 6,
                    ParameterName = "ApplicantAge",
                    Operator = ">",
                    ThresholdValue = 50,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 20,
                    Reason = "Older age life loading",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRiskRule
                {
                    Id = 7,
                    ProductDefinitionId = 6,
                    ParameterName = "SumAssured",
                    Operator = ">",
                    ThresholdValue = 100000,
                    AdjustmentType = "Flat",
                    AdjustmentValue = 50,
                    Reason = "Medical examination required surcharge",
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<ProductRider>(entity =>
        {
            entity.HasIndex(x => new { x.ProductDefinitionId, x.RiderCode }).IsUnique();
            entity.Property(x => x.RiderCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.AdjustmentType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.AdjustmentValue).HasPrecision(18, 2);

            entity.HasOne(x => x.ProductDefinition)
                .WithMany(x => x.Riders)
                .HasForeignKey(x => x.ProductDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new ProductRider
                {
                    Id = 1,
                    ProductDefinitionId = 1,
                    RiderCode = "WINDSHIELD",
                    Name = "Windscreen Cover",
                    AdjustmentType = "Flat",
                    AdjustmentValue = 20,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 2,
                    ProductDefinitionId = 2,
                    RiderCode = "FAMILY_PLUS",
                    Name = "Family Plus Rider",
                    AdjustmentType = "Percent",
                    AdjustmentValue = 5,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 3,
                    ProductDefinitionId = 3,
                    RiderCode = "EXCESS_BUY",
                    Name = "Excess Buy-Back",
                    AdjustmentType = "Flat",
                    AdjustmentValue = 50,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 4,
                    ProductDefinitionId = 3,
                    RiderCode = "TOWING",
                    Name = "24hr Roadside Assistance & Towing",
                    AdjustmentType = "Flat",
                    AdjustmentValue = 35,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 5,
                    ProductDefinitionId = 6,
                    RiderCode = "DBL_INDEM",
                    Name = "Accidental Death Double Indemnity",
                    AdjustmentType = "Percent",
                    AdjustmentValue = 3,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 6,
                    ProductDefinitionId = 7,
                    RiderCode = "MATERNITY",
                    Name = "Maternity Benefit",
                    AdjustmentType = "Flat",
                    AdjustmentValue = 100,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 7,
                    ProductDefinitionId = 7,
                    RiderCode = "DENTAL",
                    Name = "Dental & Optical Cover",
                    AdjustmentType = "Flat",
                    AdjustmentValue = 60,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductRider
                {
                    Id = 8,
                    ProductDefinitionId = 9,
                    RiderCode = "EXTREME_SP",
                    Name = "Extreme Sports Cover",
                    AdjustmentType = "Percent",
                    AdjustmentValue = 15,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                });
        });
    }
}
