// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           SeverityAssessmentInferenceTests.cs
//  Created:        2026-08-09
//  Description:    Phase 2.9: proves .../severity-assessment/assess
//                  returns a real AiInferenceEnvelope (deterministic
//                  hashes, correct model name) alongside the real
//                  assessment result, through the real HTTP pipeline.
// ============================================================================
using System.Net.Http.Json;
using System.Text.Json;

using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class SeverityAssessmentInferenceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Assess_ReturnsResultAndInferenceEnvelope()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var request = new
        {
            hazardType = "Fire",
            employeeInjuryCount = 1,
            machineFailureCount = 0,
            eventChainLength = 2,
            digitalTwinSimulatedSeverity = "Severe",
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("api/v1/ui/engines/severity-assessment/assess", request, JsonOptions);

        response.EnsureSuccessStatusCode();
        JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement inference = body.RootElement.GetProperty("inference");
        inference.GetProperty("modelName").GetString().Should().Be("SeverityAssessmentEngine");
        inference.GetProperty("inputHash").GetString().Should().NotBeNullOrWhiteSpace();
        inference.GetProperty("outputHash").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("result").GetProperty("finalAssessedSeverity").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Assess_SameInputTwice_ProducesTheSameInputHash()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var request = new
        {
            hazardType = "Fire",
            employeeInjuryCount = 1,
            machineFailureCount = 0,
            eventChainLength = 2,
            digitalTwinSimulatedSeverity = "Severe",
        };

        HttpResponseMessage responseA = await client.PostAsJsonAsync("api/v1/ui/engines/severity-assessment/assess", request, JsonOptions);
        HttpResponseMessage responseB = await client.PostAsJsonAsync("api/v1/ui/engines/severity-assessment/assess", request, JsonOptions);

        JsonDocument bodyA = JsonDocument.Parse(await responseA.Content.ReadAsStringAsync());
        JsonDocument bodyB = JsonDocument.Parse(await responseB.Content.ReadAsStringAsync());
        bodyA.RootElement.GetProperty("inference").GetProperty("inputHash").GetString()
            .Should().Be(bodyB.RootElement.GetProperty("inference").GetProperty("inputHash").GetString());
    }
}
