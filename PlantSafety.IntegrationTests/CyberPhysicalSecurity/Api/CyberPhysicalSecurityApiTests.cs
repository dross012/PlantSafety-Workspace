// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.CyberPhysicalSecurity
//  Layer:          IntegrationTests
//  File:           CyberPhysicalSecurityApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Smoke + Security (access-control) integration tests for all five
//                  CyberPhysicalSecurity V1 controllers, mirroring the unauthenticated-401 pattern
//                  established by PlantSafety.IntegrationTests.Financials.Api.FinancialsApiTests
//                  (this repo has no authenticated-JWT integration-test helper yet, so authenticated
//                  round-trip API scenarios are a documented gap -- see the write-up note below).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.CyberPhysicalSecurity.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CyberPhysicalSecurityApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Theory]
    [InlineData("api/v1/cyberphysicalsecurity/ot-assets")]
    [InlineData("api/v1/cyberphysicalsecurity/control-change-events")]
    [InlineData("api/v1/cyberphysicalsecurity/sensor-integrity-checks")]
    [InlineData("api/v1/cyberphysicalsecurity/sabotage-indicators")]
    [InlineData("api/v1/cyberphysicalsecurity/incidents")]
    public async Task PostWriteEndpoint_WithoutAuth_ShouldReturn401(string route)
    {
        var response = await Client.PostAsJsonAsync(route, new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("api/v1/cyberphysicalsecurity/ot-assets/00000000-0000-0000-0000-000000000001")]
    [InlineData("api/v1/cyberphysicalsecurity/control-change-events/unauthorized")]
    [InlineData("api/v1/cyberphysicalsecurity/sensor-integrity-checks/assets/00000000-0000-0000-0000-000000000001/history")]
    [InlineData("api/v1/cyberphysicalsecurity/sabotage-indicators/active")]
    [InlineData("api/v1/cyberphysicalsecurity/incidents/00000000-0000-0000-0000-000000000001")]
    public async Task GetReadEndpoint_WithoutAuth_ShouldReturn401(string route)
    {
        var response = await Client.GetAsync(route);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CloseIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"api/v1/cyberphysicalsecurity/incidents/{Guid.NewGuid()}/close", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FlagUnauthorizedControlChange_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsync(
            $"api/v1/cyberphysicalsecurity/control-change-events/{Guid.NewGuid()}/flag-unauthorized", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OtAssetsRoute_WithoutAuth_Returns401NotNotFound()
    {
        // Compatibility/Backward-Compatibility smoke check: the route must exist and be reachable
        // (routing succeeds) and must short-circuit at authentication middleware -- never a 404
        // (route missing), regardless of whether the module's Infrastructure DI wiring has been
        // linked into the composition root yet (auth middleware runs before DI resolution).
        var response = await Client.GetAsync("api/v1/cyberphysicalsecurity/ot-assets/00000000-0000-0000-0000-000000000001");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, "the route must be registered");
    }
}
