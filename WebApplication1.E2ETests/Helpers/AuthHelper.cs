using Microsoft.Playwright;

namespace WebApplication1.E2ETests.Helpers;

/// <summary>
/// Encapsulates the ASP.NET Identity login flow so every test can reach
/// authenticated pages without repeating the same steps.
/// </summary>
public static class AuthHelper
{
    /// <summary>
    /// Navigates to the login page and signs in with the configured test credentials.
    /// Throws if the login page still shows an error after submission.
    /// </summary>
    public static async Task LoginAsync(
        IPage page,
        string email = null,
        string password = null)
    {
        email ??= TestConfiguration.TestEmail;
        password ??= TestConfiguration.TestPassword;

        await page.GotoAsync($"{TestConfiguration.BaseUrl}/Identity/Account/Login");

        await page.Locator("#Input_Email").FillAsync(email);
        await page.Locator("#Input_Password").FillAsync(password);
        await page.Locator("[type=submit]").ClickAsync();

        // If the page still contains a validation summary, credentials are wrong.
        var errorVisible = await page.Locator(".validation-summary-errors").IsVisibleAsync();
        if (errorVisible)
        {
            var message = await page.Locator(".validation-summary-errors").InnerTextAsync();
            throw new InvalidOperationException(
                $"Login failed for '{email}'. Error: {message}. " +
                "Ensure a confirmed user exists — see TestConfiguration.cs.");
        }
    }

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    public static async Task LogoutAsync(IPage page)
    {
        await page.GotoAsync($"{TestConfiguration.BaseUrl}/Identity/Account/Logout");
        // The default Identity logout page has a POST form; submit it.
        var submitButton = page.Locator("[type=submit]");
        if (await submitButton.IsVisibleAsync())
        {
            await submitButton.ClickAsync();
        }
    }
}
