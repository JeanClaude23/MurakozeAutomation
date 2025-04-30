using Microsoft.Playwright;

namespace MurakozeAutomation;

[TestFixture]
public class BranchCreationTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private readonly string _username = "King3";
    private readonly string _password = "J5Zx6~X9\"z";
    private readonly string _baseUrl = "https://stg.murakoze.rw";
    private string _branchName;

    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, // Set to true for CI/CD pipelines
            SlowMo = 500 // Helps visualize the test execution
        });
        _page = await _browser.NewPageAsync();
        _branchName = $"{Environment.UserName} Branch";
    }

    [TearDown]
    public async Task Teardown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Test]
    public async Task CreateAndActivateBranch()
    {
        try
        {
            // 1. Login to the application
            await LoginAsync();

            // 2. Navigate to Administration menu
            await NavigateToAdministration();

            // 3. Create new branch
            await CreateNewBranch();

            // 4. Activate the branch
            await ActivateBranch();

            // 5. Validate branch appears in the list
            await ValidateBranchCreation();

            // 6. Logout from the system
            await LogoutAsync();
        }
        catch (Exception ex)
        {
            // Take screenshot on failure
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = $"failure_{DateTime.Now:yyyyMMddHHmmss}.png",
                FullPage = true
            });
            throw;
        }
    }

    private async Task LoginAsync()
    {
        await _page.GotoAsync(_baseUrl);
        
        // Wait for login page to load
        await _page.WaitForSelectorAsync("#username", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });

        // Fill credentials
        await _page.FillAsync("#username", _username);
        await _page.FillAsync("#password", _password);

        // Click login button
        await _page.ClickAsync("button[type='submit']");

        // Verify successful login by waiting for dashboard element
        await _page.WaitForSelectorAsync(".dashboard", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    private async Task NavigateToAdministration()
    {
        // Click on administration menu
        await _page.ClickAsync("text=Administration");
        
        // Wait for administration page to load
        await _page.WaitForSelectorAsync(".administration-header", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    private async Task CreateNewBranch()
    {
        // Click on branch management
        await _page.ClickAsync("text=Branch Management");
        
        // Click add new branch button
        await _page.ClickAsync("text=Add New Branch");
        
        // Fill branch details
        await _page.FillAsync("#branchName", _branchName);
        await _page.FillAsync("#branchCode", $"CODE{DateTime.Now:HHmmss}");
        
        // Submit the form
        await _page.ClickAsync("button[type='submit']");
        
        // Wait for success message
        await _page.WaitForSelectorAsync("text=Branch created successfully", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    private async Task ActivateBranch()
    {
        // Find the branch in the list and click activate
        var branchRow = _page.Locator($"tr:has-text('{_branchName}')");
        await branchRow.Locator("text=Activate").ClickAsync();
        
        // Confirm activation
        await _page.ClickAsync("text=Yes, activate");
        
        // Wait for activation confirmation
        await _page.WaitForSelectorAsync("text=Branch activated successfully", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }

    private async Task ValidateBranchCreation()
    {
        // Search for the branch
        await _page.FillAsync("#branchSearch", _branchName);
        await _page.ClickAsync("#searchButton");
        
        // Verify branch appears in the list
        var branchExists = await _page.Locator($"text={_branchName}").IsVisibleAsync();
        Assert.IsTrue(branchExists, $"Branch '{_branchName}' not found in the list");
        
        // Verify branch is active
        var activeStatus = await _page.Locator($"tr:has-text('{_branchName}') .status-active").IsVisibleAsync();
        Assert.IsTrue(activeStatus, $"Branch '{_branchName}' is not active");
    }

    private async Task LogoutAsync()
    {
        // Click user profile dropdown
        await _page.ClickAsync(".user-profile");
        
        // Click logout
        await _page.ClickAsync("text=Logout");
        
        // Verify logout by checking login page appears
        await _page.WaitForSelectorAsync("#username", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
    }
}