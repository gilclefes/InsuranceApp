using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Policies;

public sealed class CancelPolicyRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}
