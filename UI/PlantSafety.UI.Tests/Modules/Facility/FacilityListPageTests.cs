// ============================================================================
//  Project:        PlantSafety.UI.Tests
//  File:           FacilityListPageTests.cs
//  Created:        2026-08-09
//  Description:    bUnit tests for FacilityListPage -- the CRUD-list
//                  representative. Exercises the full real stack (page ->
//                  FacilityListState -> FacilityUiService -> real
//                  PlantSafetyApiClient) against a FakeHttpMessageHandler,
//                  covering the Ready/Empty/Error render branches.
// ============================================================================
using System.Net;

using AwesomeAssertions;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using PlantSafety.UI.Modules.Facility.Pages;
using PlantSafety.UI.Modules.Facility.Services;
using PlantSafety.UI.Modules.Facility.State;
using PlantSafety.UI.Services;
using PlantSafety.UI.Tests.TestSupport;

using Xunit;

namespace PlantSafety.UI.Tests.Modules.Facility;

public sealed class FacilityListPageTests : TestContext
{
    private const string FacilitiesRoute = "api/v1/ui/facilities";

    [Fact]
    public void Render_FacilitiesReturned_ShowsOneRowPerFacility()
    {
        const string json = """
        [
          {
            "facilityId": "11111111-1111-1111-1111-111111111111",
            "name": "Riverside Plant",
            "industry": { "industryName": "Chemical", "naicsCode": "325180" },
            "location": { "addressLine1": "1 Plant Rd", "city": "Akron", "stateOrProvince": "OH", "postalCode": "44301", "countryCode": "US" },
            "hazardProfile": { "activeHazardTypes": ["Fire"] },
            "hierarchy": { "buildings": [ { "buildingId": "22222222-2222-2222-2222-222222222222", "name": "Building A" } ] },
            "lifecycleStatus": "Active",
            "createdAtUtc": "2026-01-01T00:00:00Z",
            "updatedAtUtc": "2026-01-01T00:00:00Z"
          }
        ]
        """;
        RegisterFacilityServices(new FakeHttpMessageHandler().When(HttpMethod.Get, FacilitiesRoute, json));

        IRenderedComponent<FacilityListPage> component = RenderComponent<FacilityListPage>();

        component.FindAll("tbody tr").Should().HaveCount(1);
        component.Find("tbody tr td").TextContent.Should().Be("Riverside Plant");
    }

    [Fact]
    public void Render_NoFacilities_ShowsEmptyComponent()
    {
        RegisterFacilityServices(new FakeHttpMessageHandler().When(HttpMethod.Get, FacilitiesRoute, "[]"));

        IRenderedComponent<FacilityListPage> component = RenderComponent<FacilityListPage>();

        component.Find(".ps-empty p").TextContent.Should().Be("No facilities yet.");
    }

    [Fact]
    public void Render_ApiReturnsServerError_ShowsErrorComponent()
    {
        RegisterFacilityServices(new FakeHttpMessageHandler().When(HttpMethod.Get, FacilitiesRoute, "{}", HttpStatusCode.InternalServerError));

        IRenderedComponent<FacilityListPage> component = RenderComponent<FacilityListPage>();

        component.Find(".ps-error").GetAttribute("role").Should().Be("alert");
    }

    private void RegisterFacilityServices(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var apiClient = new PlantSafetyApiClient(httpClient);
        var uiService = new FacilityUiService(apiClient);
        Services.AddSingleton(new FacilityListState(uiService));
    }
}
