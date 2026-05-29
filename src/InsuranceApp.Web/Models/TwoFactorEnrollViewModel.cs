using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public class TwoFactorEnrollViewModel
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required.")]
    [Display(Name = "Verification code")]
    public string VerificationCode { get; set; } = string.Empty;
}
