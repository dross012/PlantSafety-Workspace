// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           FacilityCreatedEventTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.7: proves creating a facility actually publishes
//                  FacilityCreatedEvent through the real IModuleEventGateway
//                  resolved from the real host -- a test subscriber with no
//                  reference to the publisher receives it, end to end
//                  through a real HTTP request. Phase 4: the publish moved
//                  from FacilitiesController into FacilityService (handler-
//                  level orchestration, not controller logic), so this test
//                  now imports Shared.FacilityDomain.Infrastructure.Events'
//                  FacilityCreatedEvent instead of PlantSafety.UI.Api.Events'
//                  -- the observable behavior (an HTTP create publishes the
//                  event) is unchanged.
// ============================================================================
using System.Net.Http.Json;

using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shared.ModuleContracts.Abstractions.Events;

using PlantSafety.UI.Api.Controllers;
using Shared.FacilityDomain.Infrastructure.Events;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class FacilityCreatedEventTests
{
    [Fact]
    public async Task CreateFacility_PublishesFacilityCreatedEvent_ToAnIndependentSubscriber()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        IModuleEventGateway gateway = factory.Services.GetRequiredService<IModuleEventGateway>();
        FacilityCreatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<FacilityCreatedEvent>((evt, _) =>
        {
            received = evt;
            return Task.CompletedTask;
        });

        var request = new FacilityMetadataRequest(
            "Test Plant", "Chemical", null,
            new Shared.FacilityDomain.Abstractions.ValueObjects.FacilityLocation("1 Test Way", "Springfield", "IL", "62701", "US"));
        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/facilities", request);

        response.EnsureSuccessStatusCode();
        received.Should().NotBeNull();
        received!.Name.Should().Be("Test Plant");
        received.SourceModule.Should().Be("Facility");
    }
}
