using Microsoft.Playwright;

namespace WebApplication1.E2ETests.PageObjects;

/// <summary>
/// Page-object wrapper for /LoanProspects/Create.
/// </summary>
public class LoanProspectsCreatePage
{
    private readonly IPage _page;
    public static string Path => "/LoanProspects/Create";

    public LoanProspectsCreatePage(IPage page) => _page = page;

    public async Task GotoAsync() =>
        await _page.GotoAsync(TestConfiguration.BaseUrl + Path);

    public ILocator Heading => _page.Locator("h1");
    public ILocator NameInput => _page.Locator("#LoanProspect_Name");
    public ILocator EmailInput => _page.Locator("#LoanProspect_Email");
    public ILocator LoanAmountInput => _page.Locator("#LoanProspect_LoanAmount");
    public ILocator InterestRateInput => _page.Locator("#LoanProspect_InterestRate");
    public ILocator TermMonthsInput => _page.Locator("#LoanProspect_TermMonths");
    public ILocator IsSaveCheckbox => _page.Locator("#LoanProspect_IsSave").Last;  // There are 2 (EditorFor + asp-for), use the visible one
    public ILocator SubmitButton => _page.Locator("input[type=submit][value=Create]");
    public ILocator ConfirmationHeading => _page.Locator("h3");
    public ILocator ValidationSummary => _page.Locator("[data-valmsg-summary]");
    public ILocator BackToListLink => _page.Locator("a", new() { HasText = "Back to List" });

    public ILocator FieldError(string fieldName) =>
        _page.Locator($"[data-valmsg-for='LoanProspect.{fieldName}']");

    public async Task FillFormAsync(
        string name,
        string email,
        double loanAmount,
        double interestRate,
        int termMonths,
        bool isSave = false)
    {
        await NameInput.FillAsync(name);
        await EmailInput.FillAsync(email);
        await LoanAmountInput.FillAsync(loanAmount.ToString());
        await InterestRateInput.FillAsync(interestRate.ToString());
        await TermMonthsInput.FillAsync(termMonths.ToString());

        if (isSave)
        {
            // Click the visible checkbox (the second one rendered by the view)
            var checkboxes = _page.Locator("input[type=checkbox][name='LoanProspect.IsSave']");
            await checkboxes.Last.CheckAsync();
        }
    }

    public async Task SubmitAsync() => await SubmitButton.ClickAsync();
}
