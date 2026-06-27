// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PlanningProjects
//  Layer:          IntegrationTests
//  File:           PlanningProjectsApiTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the PlanningProjects API endpoints.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PlanningProjects.Api;

/// <summary>Integration tests for the PlanningProjects API endpoints.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PlanningProjectsApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetProjects_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/planning-projects");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
