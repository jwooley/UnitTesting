namespace WebApplication1.E2ETests;

/// <summary>
/// Configure base URL and test credentials here, or via environment variables:
///   E2E_BASE_URL   — default: https://localhost:7295
///   E2E_TEST_EMAIL — default: testuser@example.com
///   E2E_TEST_PASS  — default: Test@123456
///
/// REQUIREMENTS before running tests:
///   1. Start the WebApplication1 project (dotnet run or F5).
///   2. Ensure a confirmed Identity user exists with the credentials below.
///      You can register via /Identity/Account/Register, then confirm via the
///      link printed to the dev console (when using the dev SMTP stub).
/// </summary>
public static class TestConfiguration
{
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "https://localhost:7295";

    public static string TestEmail =>
        Environment.GetEnvironmentVariable("E2E_TEST_EMAIL") ?? "jimwooley@hotmail.com";

    public static string TestPassword =>
        Environment.GetEnvironmentVariable("E2E_TEST_PASS") ?? "P@ssw0rd";
}
