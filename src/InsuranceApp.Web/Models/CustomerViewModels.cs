using System.ComponentModel.DataAnnotations;
using InsuranceApp.Contracts.Claims;
using InsuranceApp.Contracts.Policies;
using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Contracts.Products;
using InsuranceApp.Contracts.Quotes;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Web.Models;

public sealed class RegisterViewModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(15, MinimumLength = 8)]
    [Display(Name = "Ghana Card Number")]
    public string GhanaCardNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.Date.AddYears(-18);

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the privacy and consent notice.")]
    [Display(Name = "I accept the privacy notice and consent to processing of my personal data.")]
    public bool ConsentAccepted { get; set; }
}

public sealed class ProductCatalogViewModel
{
    public IReadOnlyCollection<ProductDefinition> Products { get; set; } = Array.Empty<ProductDefinition>();
    public string? TypeFilter { get; set; }
}

public sealed class ProductDetailsViewModel
{
    public ProductDefinition Product { get; set; } = null!;
    public IReadOnlyCollection<ProductRider> Riders { get; set; } = Array.Empty<ProductRider>();
}

public sealed class GetQuoteViewModel
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Product code is required.")]
    [StringLength(32)]
    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int ProductTypeId { get; set; }

    public string ProductDescription { get; set; } = string.Empty;

    public decimal MaxCoverageAmount { get; set; }

    [Range(1, 100000000)]
    [Display(Name = "Coverage Amount (GHS)")]
    public decimal CoverageAmount { get; set; }

    [Range(1, 120)]
    [Display(Name = "Applicant Age")]
    public int ApplicantAge { get; set; } = 30;

    [Range(1, 100000000)]
    [Display(Name = "Vehicle Value (GHS)")]
    public decimal VehicleValue { get; set; }

    [Range(1, 100000000)]
    [Display(Name = "Sum Assured (GHS)")]
    public decimal SumAssured { get; set; }

    public List<RiderOption> AvailableRiders { get; set; } = new();
    public List<string> SelectedRiderCodes { get; set; } = new();

    // Helpers for the view to decide which fields to show
    public bool ShowVehicleValue => ProductTypeId == 1; // Motor only
    public bool ShowApplicantAge => ProductTypeId is 1 or 2 or 3 or 5 or 6 or 7 or 10 or 12 or 13; // not Fire, Marine, Home
    public bool ShowCoverageAmount => true; // always available
    public bool ShowSumAssured => ProductTypeId is 2 or 3 or 5 or 6 or 10 or 13; // Education, Funeral, Life, Health, PA, Micro
}

public sealed class RiderOption
{
    public string RiderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal AdjustmentValue { get; set; }
}

public sealed class QuoteResultViewModel
{
    public GenerateQuoteResponse Quote { get; set; } = new();
    public string ProductName { get; set; } = string.Empty;
}

public sealed class ConfirmPolicyViewModel
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string QuoteReference { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string ProductCode { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string ProductName { get; set; } = string.Empty;
    [Range(0.01, 100000000)]
    public decimal TotalPremium { get; set; }
    [Required(AllowEmptyStrings = false)]
    [StringLength(8)]
    public string CurrencyCode { get; set; } = "GHS";

    [Required, StringLength(100)]
    [Display(Name = "Coverage Type")]
    public string CoverageType { get; set; } = "Standard";

    [Required, DataType(DataType.Date)]
    [Display(Name = "Cover Start Date")]
    public DateTime InceptionDate { get; set; } = DateTime.UtcNow.Date;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Cover Expiry Date")]
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.Date.AddYears(1);

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    [Display(Name = "Payment Channel")]
    public string PaymentChannel { get; set; } = "MoMo";

    [Required]
    [Display(Name = "Mobile Money / Bank account reference")]
    [StringLength(64)]
    public string MandateReference { get; set; } = string.Empty;

    [Required]
    [Display(Name = "I confirm I have read the policy terms and agree to the premium amount above.")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the policy terms.")]
    public bool SignatureConfirmed { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Type your full name as digital signature")]
    public string DigitalSignature { get; set; } = string.Empty;
}

public sealed class CustomerDashboardViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public bool ProfileComplete { get; set; }
    public bool KycVerified { get; set; }
    public int ActivePolicies { get; set; }
    public int OpenClaims { get; set; }
    public decimal OutstandingPremium { get; set; }
    public IReadOnlyCollection<PolicySummaryResponse> RecentPolicies { get; set; } = Array.Empty<PolicySummaryResponse>();
    public IReadOnlyCollection<ClaimResponse> RecentClaims { get; set; } = Array.Empty<ClaimResponse>();
}

public sealed class MyPoliciesViewModel
{
    public IReadOnlyCollection<PolicySummaryResponse> Policies { get; set; } = Array.Empty<PolicySummaryResponse>();
}

public sealed class MyClaimsViewModel
{
    public IReadOnlyCollection<ClaimResponse> Claims { get; set; } = Array.Empty<ClaimResponse>();
}

public sealed class NewClaimViewModel
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Policy number is required.")]
    [StringLength(40)]
    [Display(Name = "Policy Number")]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Incident Date")]
    public DateTime IncidentDate { get; set; } = DateTime.UtcNow.Date;

    [Required, StringLength(80)]
    [Display(Name = "Claim Type")]
    public string ClaimType { get; set; } = string.Empty;

    [Range(1, 100000000)]
    [Display(Name = "Claimed Amount (GHS)")]
    public decimal ClaimedAmount { get; set; }

    [Display(Name = "Evidence URL (cloud link)")]
    [StringLength(256)]
    public string EvidenceUrl { get; set; } = string.Empty;

    public List<PolicySummaryResponse> EligiblePolicies { get; set; } = new();
}

public sealed class MyPaymentsViewModel
{
    public IReadOnlyCollection<PremiumCollectionItemResponse> Payments { get; set; } = Array.Empty<PremiumCollectionItemResponse>();
}

public sealed class MyNotificationsViewModel
{
    public IReadOnlyCollection<PolicyNotification> PolicyNotifications { get; set; } = Array.Empty<PolicyNotification>();
    public IReadOnlyCollection<ClaimNotification> ClaimNotifications { get; set; } = Array.Empty<ClaimNotification>();
}
