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
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.BaseRate).HasPrecision(10, 4);
            entity.Property(x => x.MinPremium).HasPrecision(18, 2);

            entity.HasData(
                new ProductDefinition
                {
                    Id = 1,
                    ProductCode = "MOTOR-TP",
                    Name = "Motor Third-Party",
                    ProductType = Domain.Enums.ProductType.Motor,
                    CurrencyCode = "GHS",
                    BaseRate = 0.0100m,
                    MinPremium = 120.00m,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
                },
                new ProductDefinition
                {
                    Id = 2,
                    ProductCode = "FUNERAL-STD",
                    Name = "Funeral Standard",
                    ProductType = Domain.Enums.ProductType.Funeral,
                    CurrencyCode = "GHS",
                    BaseRate = 0.0200m,
                    MinPremium = 50.00m,
                    IsActive = true,
                    CreatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAtUtc = new DateTime(2026, 5, 29, 0, 0, 0, DateTimeKind.Utc)
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
                });
        });
    }
}
