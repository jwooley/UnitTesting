using Microsoft.Playwright;
using Xunit;

namespace WebApplication1.E2ETests.Fixtures;

/// <summary>
/// xUnit collection fixture that manages a single Playwright browser instance
/// shared across all tests in the [Collection("Playwright")] collection.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; }
    public IBrowser Browser { get; private set; }

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            // Set to false locally to watch tests run:
            // Headless = false,
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Creates a fresh browser context (isolated cookies/storage) for each test.
    /// </summary>
    public async Task<IBrowserContext> NewContextAsync() =>
        await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            // Accept self-signed dev certificates.
            IgnoreHTTPSErrors = true,
        });
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
