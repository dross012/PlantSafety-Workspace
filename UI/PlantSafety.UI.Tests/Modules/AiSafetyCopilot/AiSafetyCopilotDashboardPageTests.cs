// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           AiSafetyCopilotDashboardPageTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for AiSafetyCopilotDashboardPage -- the
//                  Copilot UI representative. Covers the initial Empty
//                  prompt, submitting the form to request guidance, and the
//                  Error branch.
// ============================================================================
using System.Net;

using AwesomeAssertions;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using PlantSafety.UI.Modules.AiSafetyCopilot.Pages;
using PlantSafety.UI.Modules.AiSafetyCopilot.Services;
using PlantSafety.UI.Modules.AiSafetyCopilot.State;
using PlantSafety.UI.Services;
using PlantSafety.UI.Tests.TestSupport;

using Xunit;

namespace PlantSafety.UI.Tests.Modules.AiSafetyCopilot;

public sealed class AiSafetyCopilotDashboardPageTests : TestContext
{
    private const string GuideRoute = "api/v1/ui/engines/safety-copilot/guide";

    [Fact]
    public void Render_BeforeSubmit_ShowsEmptyPrompt()
    {
        RegisterCopilotServices(new FakeHttpMessageHandler());

        IRenderedComponent<AiSafetyCopilotDashboardPage> component = RenderComponent<AiSafetyCopilotDashboardPage>();

        component.Find(".ps-empty p").TextContent.Should().Be("Submit a context above to get tailored guidance.");
    }

    [Fact]
    public void SubmitForm_GuidanceReturned_RendersActionsAndRecommendations()
    {
        const string json = """
        {
          "actions": [ { "actionCode": "EVACUATE", "description": "Evacuate the affected zone.", "audience": "Employee" } ],
          "recommendations": [ { "title": "Notify Supervisor", "guidance": "Escalate immediately.", "audience": "Employee" } ],
          "generatedAtUtc": "2026-08-09T00:00:00Z"
        }
        """;
        RegisterCopilotServices(new FakeHttpMessageHandler().When(HttpMethod.Post, GuideRoute, json));
        IRenderedComponent<AiSafetyCopilotDashboardPage> component = RenderComponent<AiSafetyCopilotDashboardPage>();

        component.Find("form").Submit();

        component.FindAll("li").Should().HaveCount(2);
        component.Markup.Should().Contain("EVACUATE").And.Contain("Notify Supervisor");
    }

    [Fact]
    public void SubmitForm_ApiReturnsServerError_ShowsErrorComponent()
    {
        RegisterCopilotServices(new FakeHttpMessageHandler().When(HttpMethod.Post, GuideRoute, "{}", HttpStatusCode.InternalServerError));
        IRenderedComponent<AiSafetyCopilotDashboardPage> component = RenderComponent<AiSafetyCopilotDashboardPage>();

        component.Find("form").Submit();

        component.Find(".ps-error").GetAttribute("role").Should().Be("alert");
    }

    private void RegisterCopilotServices(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new PlantSafetyApiClient(httpClient);
        var uiService = new AiSafetyCopilotUiService(apiClient);
        Services.AddSingleton(new AiSafetyCopilotDashboardState(uiService));
    }
}
