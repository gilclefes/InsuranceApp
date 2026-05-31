using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Auth;

public sealed class OtpVerifyRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(64)]
    public string ChallengeId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(16)]
    public string Code { get; set; } = string.Empty;
}
