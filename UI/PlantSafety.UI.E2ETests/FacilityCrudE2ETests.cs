// ============================================================================
//  Project:        PlantSafety.UI.E2ETests
//  File:           FacilityCrudE2ETests.cs
//  Created:        2026-08-09
//  Description:    End-to-end CRUD flow: create a facility through the real
//                  UI form, land on its detail page, then confirm it shows
//                  up back on the list page. Runs against real PlantSafety.UI.Api
//                  + PlantSafety.UI.E2EHost processes and a real headless
//                  Chromium browser (Microsoft.Playwright) -- no mocks.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Playwright;

using PlantSafety.UI.E2ETests.TestSupport;

using Xunit;

namespace PlantSafety.UI.E2ETests;

[Collection(E2ECollection.Name)]
public sealed class FacilityCrudE2ETests(E2EEnvironmentFixture fixture)
{
    [Fact]
    public async Task CreateFacility_ThenViewDetailAndList_ShowsTheNewFacility()
    {
        string facilityName = $"E2E Test Plant {Guid.NewGuid():N}";
        IPage page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.HostUrl + "/facility/create");

            await page.FillAsync("#name", facilityName);
            await page.FillAsync("#industry", "Chemical");
            await page.FillAsync("#address", "1 Test Way");
            await page.FillAsync("#city", "Springfield");
            await page.FillAsync("#state", "IL");
            await page.FillAsync("#postal", "62701");
            await page.ClickAsync("button[type=submit]");

            // Blazor's client-side NavigateTo pushes history state without a full page
            // load, so the default WaitForURLAsync "Load" wait state never fires; wait
            // for the URL to change (Commit) instead.
            await page.WaitForURLAsync("**/facility/detail/**", new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });
            await page.WaitForSelectorAsync($"h1:has-text('{facilityName}')");

            await page.GotoAsync(fixture.HostUrl + "/facility/list");
            await page.WaitForSelectorAsync("tbody tr");
            (await page.Locator("tbody").TextContentAsync()).Should().Contain(facilityName);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
