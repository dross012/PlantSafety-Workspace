// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           WorkflowExecuteEndpointTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.8: proves .../workflow-orchestration/execute
//                  applies deterministic rollback rules through the real
//                  HTTP pipeline.
// ============================================================================
using System.Net.Http.Json;
using System.Text.Json;

using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class WorkflowExecuteEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Execute_MiddleStepFails_RollsBackPriorStepsAndSkipsLaterOnes()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var request = new
        {
            steps = new[]
            {
                new { stepType = "EvacuationTrigger", code = "EVAC-1", description = "Evacuate Building A" },
                new { stepType = "SopTrigger", code = "SOP-1", description = "Fire response SOP" },
                new { stepType = "ShutdownTrigger", code = "SHUTDOWN-1", description = "Shut down affected line" },
            },
            stepOutcomes = new[] { true, false, true },
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/workflow-orchestration/execute", request, JsonOptions);

        response.EnsureSuccessStatusCode();
        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement.ArrayEnumerator envelopes = body.RootElement.EnumerateArray();
        var statuses = envelopes.Select(e => e.GetProperty("status").GetString()).ToArray();
        statuses.Should().Equal("RolledBack", "Failed", "Skipped");
    }

    [Fact]
    public async Task Execute_MismatchedStepsAndOutcomes_Returns400()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var request = new
        {
            steps = new[] { new { stepType = "EvacuationTrigger", code = "EVAC-1", description = "Evacuate" } },
            stepOutcomes = Array.Empty<bool>(),
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/workflow-orchestration/execute", request, JsonOptions);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
