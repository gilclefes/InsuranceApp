using System.Text;
using System.Text.Encodings.Web;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize]
public class TwoFactorController(UserManager<ApplicationUser> userManager, UrlEncoder urlEncoder) : Controller
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string Issuer = "InsuranceApp";

    [HttpGet]
    public async Task<IActionResult> Enroll()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty;

        var model = new TwoFactorEnrollViewModel
        {
            SharedKey = FormatKey(key),
            AuthenticatorUri = string.Format(AuthenticatorUriFormat, Issuer, urlEncoder.Encode(user.Email ?? user.UserName ?? "user"), key)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(TwoFactorEnrollViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(model.VerificationCode))
        {
            ModelState.AddModelError(nameof(model.VerificationCode), "Enter the 6-digit code from your authenticator app.");
            return View(model);
        }

        var code = model.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var valid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
        if (!valid)
        {
            ModelState.AddModelError(nameof(model.VerificationCode), "Verification code is invalid.");
            return View(model);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        TempData["StatusMessage"] = "Two-factor authentication is now enabled for your account.";
        return RedirectToAction("Status");
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");
        ViewBag.IsEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        ViewBag.StatusMessage = TempData["StatusMessage"];
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");
        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        TempData["StatusMessage"] = "Two-factor authentication disabled.";
        return RedirectToAction("Status");
    }

    private static string FormatKey(string key)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < key.Length; i += 4)
        {
            sb.Append(key.AsSpan(i, Math.Min(4, key.Length - i))).Append(' ');
        }
        return sb.ToString().Trim().ToUpperInvariant();
    }
}
