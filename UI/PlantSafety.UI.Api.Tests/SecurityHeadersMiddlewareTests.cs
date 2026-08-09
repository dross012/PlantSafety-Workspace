// ============================================================================
//  Project:        PlantSafety.UI.Api.Tests
//  File:           SecurityHeadersMiddlewareTests.cs
//  Created:        2026-08-09
//  Description:    Phase 3.9: proves the baseline security response
//                  headers are present on a real response through the
//                  full HTTP pipeline.
// ============================================================================
using AwesomeAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace PlantSafety.UI.Api.Tests;

public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Get_AnyEndpoint_ResponseIncludesBaselineSecurityHeaders()
    {
        using var factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("api/v1/ui/facilities");

        response.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().ContainSingle().Which.Should().Be("DENY");
        response.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("strict-origin-when-cross-origin");
        response.Headers.GetValues("X-Permitted-Cross-Domain-Policies").Should().ContainSingle().Which.Should().Be("none");
    }
}
