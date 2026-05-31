using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Auth;

public sealed class RefreshTokenRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(2048)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(64)]
    public string SessionId { get; set; } = string.Empty;
}
