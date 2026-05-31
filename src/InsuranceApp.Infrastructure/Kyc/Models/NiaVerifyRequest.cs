using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Infrastructure.Kyc.Models;

public sealed class NiaVerifyRequest
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

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string DateOfBirth { get; set; } = string.Empty;
}
