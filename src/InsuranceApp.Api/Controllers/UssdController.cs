using InsuranceApp.Contracts.Ussd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

/// <summary>
/// USSD façade stub. Designed to integrate with USSD gateways
/// (Africa's Talking, Hubtel) that POST session-state per keypress with
/// the user's breadcrumb in <c>Text</c> (e.g. "1*2*3"). Returns plain-text
/// menus; the gateway converts to USSD wire format.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/ussd")]
public class UssdController : ControllerBase
{
    [HttpPost]
    public ActionResult<UssdResponse> Handle([FromBody] UssdRequest request)
    {
        var steps = string.IsNullOrWhiteSpace(request.Text)
            ? Array.Empty<string>()
            : request.Text.Split('*', StringSplitOptions.RemoveEmptyEntries);

        return Ok(Route(steps));
    }

    private static UssdResponse Route(IReadOnlyList<string> steps)
    {
        if (steps.Count == 0)
        {
            return Continue(
                "Welcome to InsuranceApp",
                "1. Get a quote",
                "2. Pay premium",
                "3. File a claim",
                "4. Policy status",
                "0. Exit");
        }

        return steps[0] switch
        {
            "1" => QuoteFlow(steps),
            "2" => Continue("Premium payment", "Enter policy number:"),
            "3" => Continue("File claim", "Enter policy number:"),
            "4" => Continue("Policy status", "Enter policy number:"),
            "0" => End("Goodbye."),
            _ => End("Invalid option.")
        };
    }

    private static UssdResponse QuoteFlow(IReadOnlyList<string> steps)
    {
        if (steps.Count == 1)
        {
            return Continue("Choose product", "1. Motor", "2. Health", "3. Life", "4. Home");
        }

        if (steps.Count == 2)
        {
            return Continue("Enter sum insured (GHS):");
        }

        if (steps.Count >= 3 && decimal.TryParse(steps[2], out var sum))
        {
            var premium = Math.Round(sum * 0.05m, 2);
            return End($"Indicative annual premium: GHS {premium:N2}. An agent will call you to complete enrolment.");
        }

        return End("Invalid amount.");
    }

    private static UssdResponse Continue(params string[] lines) =>
        new() { Message = string.Join('\n', lines), EndSession = false };

    private static UssdResponse End(string message) =>
        new() { Message = message, EndSession = true };
}
