using Microsoft.Playwright;
using System.Diagnostics;
using System.Net;
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
    private Process _webAppProcess;
    private bool _startedWebApp;

    public async ValueTask InitializeAsync()
    {
        _startedWebApp = await EnsureWebApplicationRunningAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            // Set to false locally to watch tests run:
            // Headless = false,
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();

        if (_startedWebApp && _webAppProcess is { HasExited: false })
        {
            _webAppProcess.Kill(entireProcessTree: true);
            _webAppProcess.Dispose();
        }
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

    private static async Task<bool> IsBaseUrlReachableAsync()
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };

        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(2),
        };

        try
        {
            using var response = await client.GetAsync(TestConfiguration.BaseUrl);
            return response.StatusCode != HttpStatusCode.NotFound;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> EnsureWebApplicationRunningAsync()
    {
        if (await IsBaseUrlReachableAsync())
        {
            return false;
        }

        var solutionRoot = FindSolutionRoot(AppContext.BaseDirectory);
        var projectPath = Path.Combine(solutionRoot, "WebApplication1", "WebApplication1.csproj");

        _webAppProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\"",
                WorkingDirectory = solutionRoot,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        _webAppProcess.Start();

        var timeoutAt = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < timeoutAt)
        {
            if (_webAppProcess.HasExited)
            {
                throw new InvalidOperationException("WebApplication1 exited before the E2E tests could connect.");
            }

            if (await IsBaseUrlReachableAsync())
            {
                return true;
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"WebApplication1 did not become reachable at '{TestConfiguration.BaseUrl}' within 60 seconds.");
    }

    private static string FindSolutionRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WebApplication1.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find solution root containing WebApplication1.sln.");
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
