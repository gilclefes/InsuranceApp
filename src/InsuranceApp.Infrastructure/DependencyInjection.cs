using InsuranceApp.Application.Common;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Kyc;
using InsuranceApp.Infrastructure.Notifications;
using InsuranceApp.Infrastructure.Onboarding;
using InsuranceApp.Infrastructure.Claims;
using InsuranceApp.Infrastructure.Compliance;
using InsuranceApp.Infrastructure.Policies;
using InsuranceApp.Infrastructure.Payments;
using InsuranceApp.Infrastructure.Payments.Providers;
using InsuranceApp.Infrastructure.Payouts;
using InsuranceApp.Infrastructure.Privacy;
using InsuranceApp.Infrastructure.Operations;
using InsuranceApp.Infrastructure.Quotes;
using InsuranceApp.Infrastructure.Otp;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Security;
using InsuranceApp.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var mySqlServerVersion = new MySqlServerVersion(new Version(8, 0, 36));

        services.AddDbContextPool<InsuranceDbContext>(options =>
        {
            options.UseMySql(connectionString, mySqlServerVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<InsuranceDbContext>()
            .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>(TokenOptions.DefaultAuthenticatorProvider);

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var authOptions = configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
        var otpOptions = configuration.GetSection("Otp").Get<OtpOptions>() ?? new OtpOptions();
        var otpProviderOptions = configuration.GetSection("OtpProvider").Get<OtpProviderOptions>() ?? new OtpProviderOptions();
        var niaOptions = configuration.GetSection("Nia").Get<NiaOptions>() ?? new NiaOptions();

        services.AddSingleton(Options.Create(jwtOptions));
        services.AddSingleton(Options.Create(authOptions));
        services.AddSingleton(Options.Create(otpOptions));
        services.AddSingleton(Options.Create(otpProviderOptions));
        services.AddSingleton(Options.Create(niaOptions));

        services.AddHttpClient(nameof(NiaApiClient), client =>
        {
            if (!string.IsNullOrWhiteSpace(niaOptions.BaseUrl))
            {
                client.BaseAddress = new Uri(niaOptions.BaseUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(Math.Max(3, niaOptions.TimeoutSeconds));
            if (!string.IsNullOrWhiteSpace(niaOptions.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", niaOptions.ApiKey);
            }
        });

        services.AddHttpClient(nameof(HttpOtpSender), client =>
        {
            if (!string.IsNullOrWhiteSpace(otpProviderOptions.BaseUrl))
            {
                client.BaseAddress = new Uri(otpProviderOptions.BaseUrl);
            }

            client.Timeout = TimeSpan.FromSeconds(10);
            if (!string.IsNullOrWhiteSpace(otpProviderOptions.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", otpProviderOptions.ApiKey);
            }
        });

        services.AddSingleton<JwtTokenFactory>();
        services.AddScoped<LoginRiskEvaluator>();
        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<IIdentityAuditLogger, DbIdentityAuditLogger>();
        services.AddScoped<IIdentityAuditReadService, IdentityAuditReadService>();
        services.AddScoped<IComplianceReportingService, ComplianceReportingService>();
        services.AddScoped<IFinanceReportingService, InsuranceApp.Infrastructure.Reporting.FinanceReportingService>();
        services.AddScoped<IAgentPortalService, InsuranceApp.Infrastructure.Agents.AgentPortalService>();
        services.Configure<InsuranceApp.Infrastructure.Agents.AgentCommissionOptions>(configuration.GetSection("AgentCommission"));
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IQuoteService, RuleBasedQuoteService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IPayoutService, PayoutService>();
        services.AddScoped<IDataRetentionService, DataRetentionService>();
        services.AddScoped<IOperationsHardeningService, OperationsHardeningService>();
        services.AddScoped<IPolicyIssuanceService, QuotePolicyIssuanceService>();
        services.AddScoped<IPremiumCollectionService, PremiumCollectionService>();
        services.AddSingleton<LoggingOtpSender>();
        services.AddSingleton<IOtpSender, HttpOtpSender>();
        services.AddScoped<INiaApiClient, NiaApiClient>();
        services.AddScoped<IKycService, NiaKycService>();

        services.Configure<PaymentGatewayOptions>(configuration.GetSection("PaymentGateways"));
        services.Configure<NotificationOptions>(configuration.GetSection("Notifications"));
        services.Configure<ColumnEncryptionOptions>(configuration.GetSection("ColumnEncryption"));
        services.AddSingleton<ColumnEncryptionService>();

        services.Configure<ObjectStorageOptions>(configuration.GetSection("ObjectStorage"));
        var storageProvider = configuration.GetSection("ObjectStorage")["Provider"]?.ToUpperInvariant() ?? "LOCAL";
        if (storageProvider == "S3")
        {
            services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        }
        else
        {
            services.AddSingleton<IObjectStorage, LocalObjectStorage>();
        }

        services.AddHttpClient(nameof(HubtelPaymentGateway), (sp, client) =>
        {
            var cfg = sp.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value.Hubtel;
            if (!string.IsNullOrWhiteSpace(cfg.BaseUrl)) client.BaseAddress = new Uri(cfg.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(3, cfg.TimeoutSeconds));
        });
        services.AddHttpClient(nameof(PaystackPaymentGateway), (sp, client) =>
        {
            var cfg = sp.GetRequiredService<IOptions<PaymentGatewayOptions>>().Value.Paystack;
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(cfg.BaseUrl) ? "https://api.paystack.co" : cfg.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(3, cfg.TimeoutSeconds));
        });
        services.AddHttpClient(nameof(HubtelSmsSender));
        services.AddHttpClient(nameof(MetaWhatsAppSender));

        services.AddSingleton<SimulatedPaymentGateway>();
        services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<SimulatedPaymentGateway>());
        services.AddSingleton<IPaymentGateway, HubtelPaymentGateway>();
        services.AddSingleton<IPaymentGateway, PaystackPaymentGateway>();
        services.AddSingleton<IPaymentGateway, GhIpssAchPaymentGateway>();
        services.AddSingleton<IPaymentGatewayRouter, PaymentGatewayRouter>();
        services.AddSingleton<IWebhookSignatureValidator, HmacWebhookSignatureValidator>();

        services.AddSingleton<LoggingSmsSender>();
        services.AddSingleton<LoggingEmailSender>();
        services.AddSingleton<LoggingWhatsAppSender>();
        services.AddSingleton<ISmsSender, HubtelSmsSender>();
        services.AddSingleton<ISmsSender>(sp => sp.GetRequiredService<LoggingSmsSender>());
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<LoggingEmailSender>());
        services.AddSingleton<IWhatsAppSender, MetaWhatsAppSender>();
        services.AddSingleton<IWhatsAppSender>(sp => sp.GetRequiredService<LoggingWhatsAppSender>());
        services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}
