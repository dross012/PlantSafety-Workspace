// ============================================================================
//  Project:        PlantSafety.UI.E2ETests
//  File:           AiSeverityAssessmentConsoleE2ETests.cs
//  Created:        2026-08-09
//  Description:    End-to-end check for the engine-console pattern: load the
//                  AI Severity Assessment console, click Run against the
//                  prefilled sample JSON, and confirm the real deterministic
//                  engine (AI.SeverityAssessment) returns a result panel.
// ============================================================================
using AwesomeAssertions;

using Microsoft.Playwright;

using PlantSafety.UI.E2ETests.TestSupport;

using Xunit;

namespace PlantSafety.UI.E2ETests;

[Collection(E2ECollection.Name)]
public sealed class AiSeverityAssessmentConsoleE2ETests(E2EEnvironmentFixture fixture)
{
    [Fact]
    public async Task ClickRun_WithSampleInput_ShowsFinalSeverityInResultPanel()
    {
        IPage page = await fixture.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync(fixture.HostUrl + "/ai-severity-assessment/console");
            await page.ClickAsync(".ps-engine-console__run");
            await page.WaitForSelectorAsync(".ps-engine-console__result pre");

            string resultJson = await page.Locator(".ps-engine-console__result pre").TextContentAsync() ?? string.Empty;
            resultJson.Should().Contain("Severity");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
