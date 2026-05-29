namespace InsuranceApp.Contracts.Policies;

public sealed class RenewalReminderRunResponse
{
    public int PoliciesScanned { get; set; }
    public int RemindersSent { get; set; }
}
