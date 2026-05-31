using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Kyc;

public sealed class GhanaCardVerificationRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(15, MinimumLength = 8)]
    public string GhanaCardNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
}
