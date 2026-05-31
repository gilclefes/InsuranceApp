using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Claims;

public sealed class AssignClaimRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(64)]
    public string AdjusterUserId { get; set; } = string.Empty;

    [StringLength(250)]
    public string Note { get; set; } = string.Empty;
}