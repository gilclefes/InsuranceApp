using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Auth;

public sealed class LoginRequest
{
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(64)]
    public string OtpChallengeId { get; set; } = string.Empty;

    [StringLength(16)]
    public string OtpCode { get; set; } = string.Empty;
}
