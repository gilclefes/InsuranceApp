using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Kyc;
using InsuranceApp.Contracts.Onboarding;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceApp.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IKycService kycService,
    IOnboardingService onboardingService,
    ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Customer");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View(model);
        }

        const string customerRole = "Customer";
        if (!await roleManager.RoleExistsAsync(customerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(customerRole));
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = model.PhoneNumber.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var err in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        await userManager.AddToRoleAsync(user, customerRole);

        // KYC verification against NIA — best-effort; failure does not block onboarding (status falls back to Pending).
        try
        {
            var kyc = await kycService.VerifyGhanaCardAsync(new GhanaCardVerificationRequest
            {
                GhanaCardNumber = model.GhanaCardNumber.Trim(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = model.DateOfBirth
            }, cancellationToken);

            TempData["KycStatus"] = kyc.VerificationStatus;
            TempData["KycMessage"] = kyc.Message;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "KYC verification failed at registration for {Email}.", normalizedEmail);
            TempData["KycStatus"] = "Pending";
        }

        try
        {
            await onboardingService.CaptureProfileAsync(new CaptureProfileRequest
            {
                Email = normalizedEmail,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                GhanaCardNumber = model.GhanaCardNumber.Trim(),
                ConsentAccepted = true
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile capture failed for {Email}.", normalizedEmail);
        }

        await SignInAsync(user, customerRole);
        TempData["Success"] = "Welcome to InsuranceApp. Your account has been created.";
        return RedirectToAction("Dashboard", "Customer");
    }

    private async Task SignInAsync(ApplicationUser user, string fallbackRole)
    {
        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            roles = new[] { fallbackRole };
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!await userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? email),
            new(ClaimTypes.Email, user.Email ?? email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (!ModelState.IsValid)
        {
            TempData["AuthError"] = "Invalid logout request.";
            return RedirectToAction(nameof(Login));
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
