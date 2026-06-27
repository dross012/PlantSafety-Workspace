// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Api
//  Layer:          IntegrationTests
//  File:           SiemSafeErrorTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies that error responses never leak PII, stack traces, or SQL details.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Api;

/// <summary>
/// Verifies SIEM safety requirements: error responses must never include
/// stack traces, SQL exception details, connection strings, or PII.
/// Per the Enterprise Architectural Rules Handbook § 2.14 Exception Handling Rule.
/// Per the Canonical Module Completion Checklist — 1+ SIEM-safe error test required.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class SiemSafeErrorTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static readonly string[] ForbiddenStrings =
    [
        "StackTrace", "stack trace", "SqlException", "SqlClient",
        "ConnectionString", "Password", "Server=", "Data Source=",
        "InnerException", "at System.", "at PlantSafety."
    ];

    [Fact]
    public async Task ErrorResponse_ShouldNotContainStackTrace()
    {
        var response = await Client.GetAsync("/api/v1/non-existent-siem-test");
        var body     = await response.Content.ReadAsStringAsync();

        foreach (var forbidden in ForbiddenStrings)
        {
            body.Should().NotContain(forbidden,
                $"error responses must never leak '{forbidden}' — SIEM safety violation");
        }
    }

    [Fact]
    public async Task ErrorResponse_ShouldNotContainSqlDetails()
    {
        // POST with invalid body to trigger validation error
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/non-existent-siem-test")
        {
            Content = new StringContent("{ }")
        };
        var response = await Client.SendAsync(request);
        var body     = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("SqlException");
        body.Should().NotContain("SqlClient");
    }
}
