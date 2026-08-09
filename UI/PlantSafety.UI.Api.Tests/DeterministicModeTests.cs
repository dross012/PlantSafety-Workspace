// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           DeterministicModeTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.1 deliverable: regression tests validating
//                  deterministic outputs. Runs the real PlantSafety.UI.Api
//                  host in-process via WebApplicationFactory with
//                  DeterministicMode enabled, and confirms that two
//                  separate requests through the real HTTP pipeline (real
//                  controller -> real engine, nothing mocked) produce byte-
//                  identical timestamps -- proving the fixed clock, not
//                  the real wall clock, is actually being used end-to-end.
//                  A second fixture with DeterministicMode left off proves
//                  the default (production) path still uses the real
//                  clock, so the flag is genuinely opt-in.
// ============================================================================
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class DeterministicModeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly object ReasoningRequestBody = new
    {
        facilityStatus = "Active",
        employeeInjuryCount = 1,
        machineFailureCount = 0,
        hazardType = "Fire",
        severityLevel = "Moderate",
        applicableJurisdictions = new[] { "OSHA" },
        workflowState = "InProgress",
        enterpriseGraphConnectedFacilityCount = 0,
        digitalTwinSimulatedSeverity = "Moderate",
    };

    [Fact]
    public async Task DeterministicModeEnabled_TwoRequests_ProduceIdenticalGeneratedAtUtc()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("DeterministicMode:IsEnabled", "true"));
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage responseA = await client.PostAsJsonAsync("api/v1/ui/engines/reasoning/reason", ReasoningRequestBody, JsonOptions);
        HttpResponseMessage responseB = await client.PostAsJsonAsync("api/v1/ui/engines/reasoning/reason", ReasoningRequestBody, JsonOptions);

        JsonDocument bodyA = JsonDocument.Parse(await responseA.Content.ReadAsStringAsync());
        JsonDocument bodyB = JsonDocument.Parse(await responseB.Content.ReadAsStringAsync());

        bodyA.RootElement.GetProperty("generatedAtUtc").GetDateTime().Should().Be(bodyB.RootElement.GetProperty("generatedAtUtc").GetDateTime());
    }

    [Fact]
    public async Task DeterministicModeEnabled_MatchesTheConfiguredFixedInstant()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("DeterministicMode:IsEnabled", "true")
                   .UseSetting("DeterministicMode:FixedUtcNow", "2030-05-05T00:00:00Z"));
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/reasoning/reason", ReasoningRequestBody, JsonOptions);

        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Config binding a "...Z" string to a plain DateTime property, then round-tripping
        // it back out through System.Text.Json, can lose the Utc Kind tag along the way
        // (the value itself stays correct; only the Kind classification wobbles). Normalize
        // with ToUniversalTime() -- a no-op if Kind is already Utc, a correct conversion if
        // the reader defaulted to Local -- so the assertion checks the real instant, not Kind.
        DateTime generatedAtUtc = body.RootElement.GetProperty("generatedAtUtc").GetDateTime().ToUniversalTime();
        generatedAtUtc.Should().Be(new DateTime(2030, 5, 5, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task DeterministicModeDisabled_TwoRequests_UseTheRealWallClock()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        DateTime before = DateTime.UtcNow;

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/reasoning/reason", ReasoningRequestBody, JsonOptions);

        DateTime after = DateTime.UtcNow;
        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        DateTime generatedAtUtc = body.RootElement.GetProperty("generatedAtUtc").GetDateTime();
        generatedAtUtc.Should().BeOnOrAfter(before.AddSeconds(-1)).And.BeOnOrBefore(after.AddSeconds(1));
    }
}
