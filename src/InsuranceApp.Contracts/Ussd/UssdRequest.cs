using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Ussd;

public sealed class UssdRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(64)]
    public string SessionId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public string ServiceCode { get; set; } = string.Empty;

    [StringLength(256)]
    public string Text { get; set; } = string.Empty;
}

public sealed class UssdResponse
{
    public string Message { get; set; } = string.Empty;
    public bool EndSession { get; set; }
}
