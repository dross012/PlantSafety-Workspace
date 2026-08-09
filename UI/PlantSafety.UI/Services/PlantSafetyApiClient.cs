// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Services
//  File:           PlantSafetyApiClient.cs
//  Created:        2026-08-09
//  Description:    Purpose: the one approved typed-HttpClient wrapper every
//                  module UI service depends on (Section 10 -- "API calls:
//                  use HttpClient via approved wrapper only"). Talks to
//                  PlantSafety.UI.Api. Deterministic JSON serialization,
//                  no dynamic/reflection-based (de)serialization, generates a
//                  correlation id per call and forwards it as a header for
//                  observability (Section 14). Never used directly by
//                  components/pages/state containers -- only by
//                  Modules/&lt;Name&gt;/Services/*UiService classes.
// ============================================================================
using System.Net.Http.Json;
using System.Text.Json;

namespace PlantSafety.UI.Services;

/// <summary>The one approved typed-HttpClient wrapper for calling PlantSafety.UI.Api.</summary>
public sealed class PlantSafetyApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>GETs and deserializes a JSON response from the given relative route.</summary>
    public async Task<TResponse?> GetAsync<TResponse>(string route, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, route);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    /// <summary>POSTs a typed request body and deserializes a typed JSON response.</summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string route, TRequest body, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, route);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    /// <summary>POSTs a raw JSON request body (used by the generic engine-console pages) and returns the raw JSON response text.</summary>
    public async Task<string> PostRawJsonAsync(string route, string requestJson, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, route);
        request.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        string responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return responseText;
    }

    /// <summary>PUTs a typed request body and deserializes a typed JSON response.</summary>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string route, TRequest body, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Put, route);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string route)
    {
        var request = new HttpRequestMessage(method, route);
        request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());
        return request;
    }
}
