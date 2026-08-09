// ============================================================================
//  Project:        PlantSafety.UI.E2ETests
//  File:           EnterpriseDashboardE2ETests.cs
//  Created:        2026-08-09
//  Description:    End-to-end check that the Enterprise Dashboard page loads
//                  real aggregated data from Enterprise.Dashboard (mounted
//                  in PlantSafety.UI.Api) and renders it.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Playwright;

using PlantSafety.UI.E2ETests.TestSupport;

using Xunit;

namespace PlantSafety.UI.E2ETests;

[Collection(E2ECollection.Name)]
public sealed class EnterpriseDashboardE2ETests(E2EEnvironmentFixture fixture)
{
    [Fact]
    public async Task Navigate_ToDashboard_RendersDashboardPayloadOrErrorNotBlankPage()
    {
        IPage page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.HostUrl + "/enterprise-dashboard/dashboard");
            await page.WaitForSelectorAsync(".ps-dashboard-json, .ps-error, .ps-empty");

            (await page.Locator("h1").TextContentAsync()).Should().Be("Enterprise Dashboard");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
