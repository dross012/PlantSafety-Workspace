// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           HttpClientExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    HttpClient extensions that attach canonical test headers to every API request.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// <see cref="HttpClient"/> extension methods that attach the canonical PlantSafety
/// request headers (CorrelationId, TenantId, ActorId) to outbound test requests.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>Sends a GET and asserts HTTP 200; deserializes the body.</summary>
    public static async Task<T> GetRequiredAsync<T>(
        this HttpClient client,
        string uri,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(uri, cancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"GET {uri} should return 200 OK");
        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken))!;
    }

    /// <summary>
    /// Sends a POST with canonical PlantSafety correlation headers.
    /// Uses <paramref name="correlationId"/> when provided; generates a new one otherwise.
    /// </summary>
    public static async Task<HttpResponseMessage> PostWithHeadersAsync<T>(
        this HttpClient client,
        string uri,
        T body,
        string? correlationId = null,
        string? tenantId      = null,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId ?? Guid.NewGuid().ToString());
        request.Headers.TryAddWithoutValidation("X-Tenant-Id",      tenantId     ?? TestIds.TenantId.ToString());
        return await client.SendAsync(request, cancellationToken);
    }
}
