using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class LoginViewModel
{
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}