using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Onboarding;

public sealed class CaptureProfileRequest
{
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(AllowEmptyStrings = false)]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(15, MinimumLength = 8)]
    public string GhanaCardNumber { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Consent must be accepted.")]
    public bool ConsentAccepted { get; set; }
}
