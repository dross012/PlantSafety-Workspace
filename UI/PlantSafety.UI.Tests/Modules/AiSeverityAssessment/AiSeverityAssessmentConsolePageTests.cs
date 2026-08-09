// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           AiSeverityAssessmentConsolePageTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for AiSeverityAssessmentConsolePage -- the
//                  generic engine-console representative (EngineConsoleComponent
//                  hosted with a real state container + PlantSafetyApiClient).
// ============================================================================
using System.Net;

using AwesomeAssertions;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using PlantSafety.UI.Modules.AiSeverityAssessment.Pages;
using PlantSafety.UI.Modules.AiSeverityAssessment.Services;
using PlantSafety.UI.Modules.AiSeverityAssessment.State;
using PlantSafety.UI.Services;
using PlantSafety.UI.Tests.TestSupport;

using Xunit;

namespace PlantSafety.UI.Tests.Modules.AiSeverityAssessment;

public sealed class AiSeverityAssessmentConsolePageTests : TestContext
{
    private const string AssessRoute = "api/v1/ui/engines/severity-assessment/assess";

    [Fact]
    public void Render_OnLoad_PrefillsSampleInputJson()
    {
        RegisterSeverityServices(new FakeHttpMessageHandler());

        IRenderedComponent<AiSeverityAssessmentConsolePage> component = RenderComponent<AiSeverityAssessmentConsolePage>();

        component.Find("textarea").GetAttribute("value").Should().Contain("\"hazardType\": \"Fire\"");
    }

    [Fact]
    public void ClickRun_EngineReturnsResult_RendersResultPanel()
    {
        RegisterSeverityServices(new FakeHttpMessageHandler().When(HttpMethod.Post, AssessRoute, """{"finalSeverity":"Severe"}"""));
        IRenderedComponent<AiSeverityAssessmentConsolePage> component = RenderComponent<AiSeverityAssessmentConsolePage>();

        component.Find(".ps-engine-console__run").Click();

        component.Find(".ps-engine-console__result pre").TextContent.Should().Contain("finalSeverity");
    }

    [Fact]
    public void ClickRun_ApiReturnsServerError_ShowsErrorComponent()
    {
        RegisterSeverityServices(new FakeHttpMessageHandler().When(HttpMethod.Post, AssessRoute, "{}", HttpStatusCode.InternalServerError));
        IRenderedComponent<AiSeverityAssessmentConsolePage> component = RenderComponent<AiSeverityAssessmentConsolePage>();

        component.Find(".ps-engine-console__run").Click();

        component.Find(".ps-error__message").TextContent.Should().NotBeEmpty();
    }

    private void RegisterSeverityServices(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new PlantSafetyApiClient(httpClient);
        var uiService = new AiSeverityAssessmentUiService(apiClient);
        Services.AddSingleton(new AiSeverityAssessmentConsoleState(uiService));
    }
}
