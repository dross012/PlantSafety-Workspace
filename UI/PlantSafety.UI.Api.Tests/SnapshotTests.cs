// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           SnapshotTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.10 (Regression Safety Net): golden-file API
//                  response snapshots. Runs the real host with
//                  DeterministicMode enabled (fixed clock + seeded
//                  randomness, Phase 2.1) so timestamps/ids are
//                  reproducible, then compares the real response JSON
//                  against a committed golden file via Shared.SnapshotTesting's
//                  SnapshotHarness. To accept a real, reviewed output
//                  change as the new baseline: run with the SNAPSHOT_BASELINE=true
//                  environment variable set, review the resulting diff in
//                  __snapshots__/, then commit it.
// ============================================================================
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Shared.SnapshotTesting;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class SnapshotTests
{
    private static string SnapshotDirectory([CallerFilePath] string sourceFilePath = "") =>
        Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "__snapshots__");

    private static WebApplicationFactory<Program> CreateDeterministicFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseSetting("DeterministicMode:IsEnabled", "true")
            .UseSetting("DeterministicMode:FixedUtcNow", "2030-01-01T00:00:00Z")
            .UseSetting("DeterministicMode:Seed", "42"));

    [Fact]
    public async Task FacilityList_EmptyRepository_MatchesGoldenFile()
    {
        using WebApplicationFactory<Program> factory = CreateDeterministicFactory();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "snapshot-test-correlation-id");

        HttpResponseMessage response = await client.GetAsync("api/v1/ui/facilities");

        response.EnsureSuccessStatusCode();
        await SnapshotHarness.VerifyAsync(SnapshotDirectory(), "facility-list-empty", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SeverityAssessment_FireWithInjury_MatchesGoldenFile()
    {
        using WebApplicationFactory<Program> factory = CreateDeterministicFactory();
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "snapshot-test-correlation-id");
        var request = new
        {
            hazardType = "Fire",
            employeeInjuryCount = 1,
            machineFailureCount = 0,
            eventChainLength = 2,
            digitalTwinSimulatedSeverity = "Severe",
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/severity-assessment/assess", request);

        response.EnsureSuccessStatusCode();
        await SnapshotHarness.VerifyAsync(SnapshotDirectory(), "severity-assessment-fire-with-injury", await response.Content.ReadAsStringAsync());
    }
}
