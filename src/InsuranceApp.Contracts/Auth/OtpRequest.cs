using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Auth;

public sealed class OtpRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(255)]
    public string Destination { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(50)]
    public string Purpose { get; set; } = "Login";

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public string Channel { get; set; } = "SMS";
}
