using Microsoft.Playwright;

namespace WebApplication1.E2ETests.PageObjects;

/// <summary>
/// Page-object wrapper for /LoanProspects (Index).
/// </summary>
public class LoanProspectsIndexPage
{
    private readonly IPage _page;
    public static string Path => "/LoanProspects";

    public LoanProspectsIndexPage(IPage page) => _page = page;

    public async Task GotoAsync() =>
        await _page.GotoAsync(TestConfiguration.BaseUrl + Path);

    public ILocator Heading => _page.Locator("h1");
    public ILocator CreateNewLink => _page.Locator("a", new() { HasText = "Create New" });
    public ILocator Table => _page.Locator("table.table");
    public ILocator TableRows => _page.Locator("table.table tbody tr");

    public async Task<int> GetRowCountAsync() =>
        await TableRows.CountAsync();

    /// <summary>Returns the first row whose Last-Name cell contains <paramref name="lastName"/>.</summary>
    public ILocator RowByLastName(string lastName) =>
        _page.Locator("table.table tbody tr", new() { HasText = lastName });

    public async Task ClickCreateNewAsync() =>
        await CreateNewLink.ClickAsync();

    public async Task ClickEditForLastNameAsync(string lastName) =>
        await RowByLastName(lastName).Locator("a", new() { HasText = "Edit" }).ClickAsync();

    public async Task ClickDetailsForLastNameAsync(string lastName) =>
        await RowByLastName(lastName).Locator("a", new() { HasText = "Details" }).ClickAsync();

    public async Task ClickDeleteForLastNameAsync(string lastName) =>
        await RowByLastName(lastName).Locator("a", new() { HasText = "Delete" }).ClickAsync();
}
