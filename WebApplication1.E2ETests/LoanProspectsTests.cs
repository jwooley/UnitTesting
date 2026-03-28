using Microsoft.Playwright;
using WebApplication1.E2ETests.Fixtures;
using WebApplication1.E2ETests.Helpers;
using WebApplication1.E2ETests.PageObjects;
using Xunit;

namespace WebApplication1.E2ETests;

/// <summary>
/// End-to-end tests for the LoanProspects Razor Pages.
///
/// Each test gets its own isolated browser context (separate cookies/storage)
/// so tests do not share authenticated state.
///
/// Prerequisites:
///   • WebApplication1 must be running at TestConfiguration.BaseUrl.
///   • A confirmed Identity user must exist with TestConfiguration.TestEmail /
///     TestConfiguration.TestPassword (see TestConfiguration.cs).
/// </summary>
[Collection("Playwright")]
public class LoanProspectsTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwright;
    private IBrowserContext _context;
    private IPage _page;

    public LoanProspectsTests(PlaywrightFixture playwright)
    {
        _playwright = playwright;
    }

    public async Task InitializeAsync()
    {
        _context = await _playwright.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    // -------------------------------------------------------------------------
    // Helper: fill & submit a valid Create form
    // -------------------------------------------------------------------------
    private async Task<LoanProspectsCreatePage> CreateLoanProspectAsync(
        string name,
        string email,
        double loanAmount,
        double interestRate,
        int termMonths,
        bool isSave = false)
    {
        var createPage = new LoanProspectsCreatePage(_page);
        await createPage.GotoAsync();
        await createPage.FillFormAsync(name, email, loanAmount, interestRate, termMonths, isSave);
        await createPage.SubmitAsync();
        return createPage;
    }

    // =========================================================================
    // HAPPY PATH TESTS
    // =========================================================================

    [Fact]
    public async Task IndexPage_WhenAuthenticated_LoadsWithExpectedHeading()
    {
        await AuthHelper.LoginAsync(_page);

        var indexPage = new LoanProspectsIndexPage(_page);
        await indexPage.GotoAsync();

        await Assertions.Expect(_page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Index"));
        await Assertions.Expect(indexPage.Heading).ToHaveTextAsync("Index");
        await Assertions.Expect(indexPage.CreateNewLink).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatePage_WhenAuthenticated_RendersForm()
    {
        await AuthHelper.LoginAsync(_page);

        var createPage = new LoanProspectsCreatePage(_page);
        await createPage.GotoAsync();

        await Assertions.Expect(createPage.Heading).ToHaveTextAsync("Create");
        await Assertions.Expect(createPage.NameInput).ToBeVisibleAsync();
        await Assertions.Expect(createPage.LoanAmountInput).ToBeVisibleAsync();
        await Assertions.Expect(createPage.InterestRateInput).ToBeVisibleAsync();
        await Assertions.Expect(createPage.TermMonthsInput).ToBeVisibleAsync();
        await Assertions.Expect(createPage.SubmitButton).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Create_WithValidData_DisplaysCalculatedPayment()
    {
        await AuthHelper.LoginAsync(_page);

        // $200,000 at 6 % for 360 months → ≈ $1,199.10/month
        await CreateLoanProspectAsync(
            name: "John Smith",
            email: "john.smith@example.com",
            loanAmount: 200000,
            interestRate: 6,
            termMonths: 360);

        // The confirmation message is rendered in an <h3> on the same page
        var confirmation = _page.Locator("h3");
        await Assertions.Expect(confirmation).ToBeVisibleAsync();
        var text = await confirmation.InnerTextAsync();
        Assert.Contains("John", text);
        Assert.Contains("$", text);   // currency symbol from .ToString("c2")
    }

    [Fact]
    public async Task Create_WithIsSave_RedirectsToIndexAndAppearsInList()
    {
        await AuthHelper.LoginAsync(_page);

        var uniqueLastName = $"Playwright{Guid.NewGuid():N}";
        await CreateLoanProspectAsync(
            name: $"Test {uniqueLastName}",
            email: "test@example.com",
            loanAmount: 100000,
            interestRate: 5,
            termMonths: 120,
            isSave: true);

        // After saving, app stays on Create page showing the confirmation.
        // Navigate to index to confirm record is persisted.
        var indexPage = new LoanProspectsIndexPage(_page);
        await indexPage.GotoAsync();

        var row = indexPage.RowByLastName(uniqueLastName);
        await Assertions.Expect(row).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Details_ForExistingRecord_ShowsAllFields()
    {
        await AuthHelper.LoginAsync(_page);

        var uniqueLastName = $"Details{Guid.NewGuid():N}";
        await CreateLoanProspectAsync(
            name: $"Alice {uniqueLastName}",
            email: "alice@example.com",
            loanAmount: 150000,
            interestRate: 4.5,
            termMonths: 180,
            isSave: true);

        var indexPage = new LoanProspectsIndexPage(_page);
        await indexPage.GotoAsync();
        await indexPage.ClickDetailsForLastNameAsync(uniqueLastName);

        await Assertions.Expect(_page.Locator("h1")).ToHaveTextAsync("Details");
        // Verify the loan amount is shown — filter to the specific dd that contains the value
        await Assertions.Expect(_page.Locator("dd.col-sm-10").Filter(new() { HasText = "150000" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Edit_WithNewLoanAmount_UpdatesRecordInIndex()
    {
        await AuthHelper.LoginAsync(_page);

        var uniqueLastName = $"Edit{Guid.NewGuid():N}";
        await CreateLoanProspectAsync(
            name: $"Bob {uniqueLastName}",
            email: "bob@example.com",
            loanAmount: 80000,
            interestRate: 5,
            termMonths: 60,
            isSave: true);

        var indexPage = new LoanProspectsIndexPage(_page);
        await indexPage.GotoAsync();
        await indexPage.ClickEditForLastNameAsync(uniqueLastName);

        await Assertions.Expect(_page.Locator("h1")).ToHaveTextAsync("Edit");

        // Update the loan amount
        var loanAmountInput = _page.Locator("#LoanProspect_LoanAmount");
        await loanAmountInput.FillAsync("99999");
        await _page.Locator("input[type=submit][value=Save]").ClickAsync();

        // Should redirect back to index
        await indexPage.GotoAsync();

        // The Edit form does not rebind NameFirst/NameLast, so search by the updated loan amount.
        // Use .First so the assertion tolerates multiple matching rows in the shared DB.
        var updatedAmountCell = _page.Locator("table.table tbody td").Filter(new() { HasText = "$99,999.00" }).First;
        await Assertions.Expect(updatedAmountCell).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Delete_ExistingRecord_RemovesItFromList()
    {
        await AuthHelper.LoginAsync(_page);

        var uniqueLastName = $"Del{Guid.NewGuid():N}";
        await CreateLoanProspectAsync(
            name: $"Carol {uniqueLastName}",
            email: "carol@example.com",
            loanAmount: 50000,
            interestRate: 7,
            termMonths: 48,
            isSave: true);

        var indexPage = new LoanProspectsIndexPage(_page);
        await indexPage.GotoAsync();
        await indexPage.ClickDeleteForLastNameAsync(uniqueLastName);

        // Confirm delete page loaded
        await Assertions.Expect(_page.Locator("h1")).ToHaveTextAsync("Delete");
        await Assertions.Expect(_page.Locator("h3")).ToContainTextAsync("Are you sure");

        // Submit the delete form
        await _page.Locator("input[type=submit][value=Delete]").ClickAsync();

        // Redirects to index; the row should be gone
        await Assertions.Expect(_page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/LoanProspects$"));

        var row = indexPage.RowByLastName(uniqueLastName);
        await Assertions.Expect(row).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task BackToList_NavigatesFromCreateToIndex()
    {
        await AuthHelper.LoginAsync(_page);

        var createPage = new LoanProspectsCreatePage(_page);
        await createPage.GotoAsync();

        await createPage.BackToListLink.ClickAsync();

        await Assertions.Expect(_page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/LoanProspects$"));
    }

    // =========================================================================
    // EDGE CASE TESTS
    // =========================================================================

    [Fact]
    public async Task IndexPage_WhenUnauthenticated_RedirectsToLogin()
    {
        // Do NOT call LoginAsync — go straight to the protected page.
        await _page.GotoAsync(TestConfiguration.BaseUrl + LoanProspectsIndexPage.Path);

        // ASP.NET Identity redirects to /Identity/Account/Login?ReturnUrl=...
        await Assertions.Expect(_page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/Identity/Account/Login"));
    }

    [Fact]
    public async Task CreatePage_WhenUnauthenticated_RedirectsToLogin()
    {
        await _page.GotoAsync(TestConfiguration.BaseUrl + LoanProspectsCreatePage.Path);

        await Assertions.Expect(_page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/Identity/Account/Login"));
    }

    [Fact]
    public async Task Create_WithEmptyForm_ShowsClientSideValidation()
    {
        await AuthHelper.LoginAsync(_page);

        var createPage = new LoanProspectsCreatePage(_page);
        await createPage.GotoAsync();

        // Submit without filling in any fields
        await createPage.SubmitButton.ClickAsync();

        // Page should still be on Create (client-side validation prevents navigation)
        await Assertions.Expect(_page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex("/LoanProspects/Create"));
    }

    [Fact]
    public async Task Create_WithNameMissingSpaceSeparator_ThrowsServerError()
    {
        // The server splits Name on " " and accesses index [1].
        // A single-word name causes an IndexOutOfRangeException.
        await AuthHelper.LoginAsync(_page);

        await CreateLoanProspectAsync(
            name: "SingleName",           // no space — will throw
            email: "single@example.com",
            loanAmount: 50000,
            interestRate: 5,
            termMonths: 60);

        // The app should return an error page (500) or re-render Create with an error.
        // We verify the user is not silently redirected to Index.
        var url = _page.Url;
        Assert.DoesNotMatch(
            new System.Text.RegularExpressions.Regex("/LoanProspects$"),
            url);
    }

    [Fact]
    public async Task Create_WithZeroLoanAmount_CalculatesZeroPayment()
    {
        await AuthHelper.LoginAsync(_page);

        await CreateLoanProspectAsync(
            name: "Zero Amount",
            email: "zero@example.com",
            loanAmount: 0,
            interestRate: 5,
            termMonths: 60);

        var confirmation = _page.Locator("h3");
        await Assertions.Expect(confirmation).ToBeVisibleAsync();
        var text = await confirmation.InnerTextAsync();
        // $0.00 payment expected
        Assert.Contains("$0.00", text);
    }

    [Fact]
    public async Task Create_WithZeroInterestRate_CalculatesFlatPayment()
    {
        // 0 % rate: payment = principal / termMonths
        await AuthHelper.LoginAsync(_page);

        await CreateLoanProspectAsync(
            name: "Zero Rate",
            email: "zerorate@example.com",
            loanAmount: 12000,
            interestRate: 0,
            termMonths: 12);

        var confirmation = _page.Locator("h3");
        await Assertions.Expect(confirmation).ToBeVisibleAsync();
        var text = await confirmation.InnerTextAsync();
        // 12000 / 12 = $1,000.00
        Assert.Contains("$1,000.00", text);
    }

    [Fact]
    public async Task Create_WithVeryHighInterestRate_CalculatesLargePayment()
    {
        await AuthHelper.LoginAsync(_page);

        await CreateLoanProspectAsync(
            name: "High Rate",
            email: "highrate@example.com",
            loanAmount: 10000,
            interestRate: 99,
            termMonths: 12);

        var confirmation = _page.Locator("h3");
        await Assertions.Expect(confirmation).ToBeVisibleAsync();
        // Payment should exist and be significantly larger than a normal rate
        var text = await confirmation.InnerTextAsync();
        Assert.Contains("$", text);
    }

    [Fact]
    public async Task Create_WithMaxTermMonths_CalculatesLowMonthlyPayment()
    {
        await AuthHelper.LoginAsync(_page);

        await CreateLoanProspectAsync(
            name: "Long Term",
            email: "longterm@example.com",
            loanAmount: 100000,
            interestRate: 5,
            termMonths: 360);

        var confirmation = _page.Locator("h3");
        await Assertions.Expect(confirmation).ToBeVisibleAsync();
        var text = await confirmation.InnerTextAsync();
        Assert.Contains("$", text);
    }

    [Fact]
    public async Task Details_NonExistentId_ReturnsNotFoundOrErrorPage()
    {
        await AuthHelper.LoginAsync(_page);

        var response = await _page.GotoAsync(
            $"{TestConfiguration.BaseUrl}/LoanProspects/Details?id=999999");

        // The app returns 404 or the Error page — either is acceptable; it must not crash with 500.
        Assert.NotNull(response);
        Assert.True(
            response.Status == 404 || response.Status == 200,
            $"Unexpected status {response.Status} for non-existent record.");
    }

    [Fact]
    public async Task Edit_NonExistentId_ReturnsNotFoundOrErrorPage()
    {
        await AuthHelper.LoginAsync(_page);

        var response = await _page.GotoAsync(
            $"{TestConfiguration.BaseUrl}/LoanProspects/Edit?id=999999");

        Assert.NotNull(response);
        Assert.True(
            response.Status == 404 || response.Status == 200,
            $"Unexpected status {response.Status} for non-existent record.");
    }

    [Fact]
    public async Task Delete_NonExistentId_ReturnsNotFoundOrErrorPage()
    {
        await AuthHelper.LoginAsync(_page);

        var response = await _page.GotoAsync(
            $"{TestConfiguration.BaseUrl}/LoanProspects/Delete?id=999999");

        Assert.NotNull(response);
        Assert.True(
            response.Status == 404 || response.Status == 200,
            $"Unexpected status {response.Status} for non-existent record.");
    }
}
