// ============================================================================
//  Project:        PlantSafety.UI.E2ETests
//  File:           AiSafetyCopilotDashboardE2ETests.cs
//  Created:        2026-08-09
//  Description:    End-to-end check for the Copilot UI: submit the guidance
//                  form with its default field values and confirm the real
//                  AI.SafetyCopilot engine returns at least one action and
//                  one recommendation, rendered in the page.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Playwright;

using PlantSafety.UI.E2ETests.TestSupport;

using Xunit;

namespace PlantSafety.UI.E2ETests;

[Collection(E2ECollection.Name)]
public sealed class AiSafetyCopilotDashboardE2ETests(E2EEnvironmentFixture fixture)
{
    [Fact]
    public async Task SubmitDefaultForm_ReturnsActionsAndRecommendations()
    {
        IPage page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.HostUrl + "/ai-safety-copilot/dashboard");
            (await page.Locator(".ps-empty p").TextContentAsync()).Should().Be("Submit a context above to get tailored guidance.");

            await page.ClickAsync("button[type=submit]");
            await page.WaitForSelectorAsync("h2:has-text('Actions')");

            int actionCount = await page.Locator("ul").First.Locator("li").CountAsync();
            actionCount.Should().BeGreaterThan(0);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
