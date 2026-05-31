using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public class TwoFactorEnrollViewModel
{
    [StringLength(128)]
    public string SharedKey { get; set; } = string.Empty;

    [Url]
    [StringLength(2048)]
    public string AuthenticatorUri { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required.")]
    [Display(Name = "Verification code")]
    [StringLength(16)]
    [RegularExpression("^[0-9\\-\\s]+$", ErrorMessage = "Verification code must contain only digits.")]
    public string VerificationCode { get; set; } = string.Empty;
}
