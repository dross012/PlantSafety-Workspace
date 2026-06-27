// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.ExternalServices
//  Layer:          Infrastructure
//  File:           ExternalServiceClientBase.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for outbound HTTP clients. Enforces correlation header propagation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.ExternalServices;

/// <summary>
/// Abstract base for outbound HTTP clients. Ensures every request carries
/// the canonical <c>X-Correlation-Id</c> header and uses structured error handling.
/// </summary>
public abstract class ExternalServiceClientBase(HttpClient httpClient, ILogger logger)
    : IExternalServiceClient
{
    private const string CorrelationHeader = "X-Correlation-Id";

    /// <summary>HTTP client configured by the consuming service.</summary>
    protected HttpClient Client { get; } = httpClient;

    /// <summary>Sends a GET request to <paramref name="uri"/> with the canonical correlation header.</summary>
    protected async Task<T?> GetAsync<T>(string uri, string correlationId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.TryAddWithoutValidation(CorrelationHeader, correlationId);

        var response = await Client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("External service GET {Uri} returned {StatusCode}", uri, response.StatusCode);
            return default;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerConfiguration.Default, cancellationToken);
    }
}
