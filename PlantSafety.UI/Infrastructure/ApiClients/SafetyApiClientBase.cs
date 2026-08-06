// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstract base for all module-specific typed API clients.
//   Provides canonical GET/POST/PUT/DELETE helpers that deserialize into
//   ApiResponseEnvelope<T>, propagate correlation context, and convert
//   non-success responses into ProblemDetailsDto without leaking HTTP concerns
//   to the calling component or state container.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §2.11
// ============================================================

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Models.Common;

namespace PlantSafety.UI.Infrastructure.ApiClients;

/// <summary>
/// Base class for all PlantSafety typed API clients. Wraps <see cref="HttpClient"/>
/// with canonical response deserialization, ProblemDetails mapping, and
/// structured SIEM-safe logging.
/// </summary>
/// <remarks>
/// All derived clients MUST:
/// - Use the protected helper methods rather than calling HttpClient directly.
/// - Never expose raw HttpResponseMessage to consumers.
/// - Never expose domain objects — return module-specific DTOs only.
/// </remarks>
public abstract class SafetyApiClientBase
{
    /// <summary>The named HttpClient pre-configured with base address and delegating handlers.</summary>
    protected readonly HttpClient Http;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes the base client with a configured <see cref="HttpClient"/> and logger.
    /// </summary>
    /// <param name="http">The typed HttpClient with correlation and auth handlers applied.</param>
    /// <param name="logger">SIEM-safe structured logger scoped to the derived type.</param>
    protected SafetyApiClientBase(HttpClient http, ILogger logger)
    {
        Http = http;
        _logger = logger;
    }

    /// <summary>
    /// Issues a GET request and deserializes the response into <see cref="ApiResponseEnvelope{T}"/>.
    /// </summary>
    /// <typeparam name="T">The expected response payload type.</typeparam>
    /// <param name="relativeUri">The API-relative path (e.g., "v1/alerts").</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The deserialized envelope or a fault envelope on failure.</returns>
    protected async Task<ApiResponseEnvelope<T>> GetAsync<T>(
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Http.GetAsync(relativeUri, cancellationToken).ConfigureAwait(false);
            return await ReadEnvelopeAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<T>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Issues a POST request with a JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="relativeUri">The API-relative path.</param>
    /// <param name="payload">The request body, serialized as JSON.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The deserialized envelope or a fault envelope on failure.</returns>
    protected async Task<ApiResponseEnvelope<TResponse>> PostAsync<TRequest, TResponse>(
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Http.PostAsJsonAsync(relativeUri, payload, _serializerOptions, cancellationToken)
                                     .ConfigureAwait(false);
            return await ReadEnvelopeAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<TResponse>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Issues a PUT request with a JSON body and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="relativeUri">The API-relative path.</param>
    /// <param name="payload">The request body, serialized as JSON.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The deserialized envelope or a fault envelope on failure.</returns>
    protected async Task<ApiResponseEnvelope<TResponse>> PutAsync<TRequest, TResponse>(
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Http.PutAsJsonAsync(relativeUri, payload, _serializerOptions, cancellationToken)
                                     .ConfigureAwait(false);
            return await ReadEnvelopeAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<TResponse>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Issues a DELETE request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="relativeUri">The API-relative path.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The deserialized envelope or a fault envelope on failure.</returns>
    protected async Task<ApiResponseEnvelope<TResponse>> DeleteAsync<TResponse>(
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Http.DeleteAsync(relativeUri, cancellationToken).ConfigureAwait(false);
            return await ReadEnvelopeAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<TResponse>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    private async Task<ApiResponseEnvelope<T>> ReadEnvelopeAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            // PlantSafety.Api wraps every 2xx payload in the canonical ApiResponse<T> envelope
            // ({ success, data, errors, metadata } on the wire). Unwrap "data" here so downstream
            // state containers and components work with the plain payload type T.
            var wire = await response.Content
                .ReadFromJsonAsync<ApiEnvelopeWireDto<T>>(_serializerOptions, cancellationToken)
                .ConfigureAwait(false);

            return ApiResponseEnvelope<T>.Success(wire!.Data!);
        }

        ProblemDetailsDto? problem = null;
        try
        {
            problem = await response.Content
                .ReadFromJsonAsync<ProblemDetailsDto>(_serializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // Non-JSON error body — build minimal problem.
        }

        _logger.LogWarning(
            "API returned non-success status. Status={Status} Title={Title}",
            (int)response.StatusCode,
            problem?.Title ?? "Unknown");

        return ApiResponseEnvelope<T>.FromProblem(problem ?? ProblemDetailsDto.Unknown((int)response.StatusCode));
    }

    /// <summary>
    /// Wire shape of PlantSafety.Api's canonical <c>ApiResponse&lt;T&gt;</c> envelope
    /// (PlantSafety.Api.Models.ApiResponse&lt;T&gt;), used only to deserialize the "data"
    /// property on 2xx responses. Never exposed outside this base class.
    /// </summary>
    private sealed class ApiEnvelopeWireDto<T>
    {
        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        public bool Success { get; init; }

        /// <summary>Gets the strongly-typed data payload.</summary>
        public T? Data { get; init; }
    }
}
