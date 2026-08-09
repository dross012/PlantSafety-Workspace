// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           EnterpriseDashboardPageTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for EnterpriseDashboardPage -- the dashboard
//                  representative. Covers the Ready render branch (formatted
//                  JSON payload) and the Error branch.
// ============================================================================
using System.Net;

using AwesomeAssertions;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using PlantSafety.UI.Modules.EnterpriseDashboard.Pages;
using PlantSafety.UI.Modules.EnterpriseDashboard.Services;
using PlantSafety.UI.Modules.EnterpriseDashboard.State;
using PlantSafety.UI.Services;
using PlantSafety.UI.Tests.TestSupport;

using Xunit;

namespace PlantSafety.UI.Tests.Modules.EnterpriseDashboard;

public sealed class EnterpriseDashboardPageTests : TestContext
{
    private const string DashboardRoute = "api/v1/enterprise/dashboard";

    [Fact]
    public void Render_DashboardPayloadReturned_ShowsFormattedJson()
    {
        RegisterDashboardServices(new FakeHttpMessageHandler().When(HttpMethod.Get, DashboardRoute, """{"facilityCount":3}"""));

        IRenderedComponent<EnterpriseDashboardPage> component = RenderComponent<EnterpriseDashboardPage>();

        component.Find(".ps-dashboard-json").TextContent.Should().Contain("facilityCount");
    }

    [Fact]
    public void Render_ApiReturnsServerError_ShowsErrorComponentWithRetry()
    {
        RegisterDashboardServices(new FakeHttpMessageHandler().When(HttpMethod.Get, DashboardRoute, "{}", HttpStatusCode.InternalServerError));

        IRenderedComponent<EnterpriseDashboardPage> component = RenderComponent<EnterpriseDashboardPage>();

        component.FindAll(".ps-error__retry").Should().ContainSingle();
    }

    private void RegisterDashboardServices(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new PlantSafetyApiClient(httpClient);
        var uiService = new EnterpriseDashboardUiService(apiClient);
        Services.AddSingleton(new EnterpriseDashboardState(uiService));
    }
}
