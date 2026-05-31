using InsuranceApp.Contracts.Auth;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Onboarding;

public sealed class AgentOnboardCustomerRequest
{
    [Required]
    public RegisterRequest Register { get; set; } = new();

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.Date.AddYears(-18);

    [Required(AllowEmptyStrings = false)]
    [StringLength(15, MinimumLength = 8)]
    public string GhanaCardNumber { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Consent must be accepted.")]
    public bool ConsentAccepted { get; set; }
}
