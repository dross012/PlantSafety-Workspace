// ============================================================================
// File: ApiResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Canonical API response envelope used by all endpoints in PlantSafety.Api.
// ============================================================================
//
// Onboarding Notes:
// - Every controller action and minimal API handler must return ApiResponse<T>.
// - The envelope enforces a consistent contract: success flag, typed data, errors list,
//   and metadata (correlationId, timestamp, version). See Section 9.1 of the Architecture Spec.
// - Use ApiResponse<T>.Ok() for 200-level responses; ApiResponse<T>.Fail() for error responses.
//
// Rationale:
// - Provides a single, versioned response shape across all 16 bounded contexts.
// - CorrelationId propagation enables end-to-end distributed tracing linkage.
//
// Future Extension Seams:
// - Add pagination metadata to the envelope for list endpoints.
// - Add warnings collection for partial-success scenarios.
//
// ============================================================================

namespace PlantSafety.Api.Models;

/// <summary>
/// Canonical response envelope wrapping all PlantSafety API responses.
/// Conforms to the standard response contract defined in Architecture Specification §9.1.
/// </summary>
/// <typeparam name="T">The type of the data payload returned by the endpoint.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the strongly-typed data payload. Null on failure.</summary>
    public T? Data { get; init; }

    /// <summary>Gets the list of human-readable error messages. Empty on success.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Gets the response metadata including correlation ID, timestamp, and API version.</summary>
    public ResponseMetadata Metadata { get; init; } = default!;

    /// <summary>
    /// Creates a successful response envelope containing the given data payload.
    /// </summary>
    /// <param name="data">The data returned from the operation.</param>
    /// <param name="correlationId">The correlation ID propagated from the X-Correlation-ID request header.</param>
    /// <param name="version">The API version string. Defaults to "1.0".</param>
    /// <returns>A populated <see cref="ApiResponse{T}"/> with <see cref="Success"/> set to true.</returns>
    public static ApiResponse<T> Ok(T data, string correlationId, string version = "1.0") =>
        new()
        {
            Success = true,
            Data = data,
            Errors = [],
            Metadata = ResponseMetadata.Create(correlationId, version)
        };

    /// <summary>
    /// Creates a failure response envelope containing one or more error messages.
    /// </summary>
    /// <param name="errors">The collection of error messages describing the failure.</param>
    /// <param name="correlationId">The correlation ID propagated from the X-Correlation-ID request header.</param>
    /// <param name="version">The API version string. Defaults to "1.0".</param>
    /// <returns>A populated <see cref="ApiResponse{T}"/> with <see cref="Success"/> set to false.</returns>
    public static ApiResponse<T> Fail(IEnumerable<string> errors, string correlationId, string version = "1.0") =>
        new()
        {
            Success = false,
            Data = default,
            Errors = errors.ToList().AsReadOnly(),
            Metadata = ResponseMetadata.Create(correlationId, version)
        };

    /// <summary>
    /// Creates a failure response envelope from a single error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="correlationId">The correlation ID propagated from the X-Correlation-ID request header.</param>
    /// <param name="version">The API version string. Defaults to "1.0".</param>
    /// <returns>A populated <see cref="ApiResponse{T}"/> with <see cref="Success"/> set to false.</returns>
    public static ApiResponse<T> Fail(string error, string correlationId, string version = "1.0") =>
        Fail([error], correlationId, version);
}

/// <summary>
/// Metadata included in every API response to support observability and client debugging.
/// </summary>
/// <param name="CorrelationId">The correlation ID echoed from the X-Correlation-ID request header.</param>
/// <param name="Timestamp">The UTC timestamp at which the response was generated.</param>
/// <param name="Version">The API version that produced this response.</param>
public sealed record ResponseMetadata(string CorrelationId, DateTime Timestamp, string Version)
{
    /// <summary>
    /// Creates a <see cref="ResponseMetadata"/> instance with the current UTC timestamp.
    /// </summary>
    /// <param name="correlationId">The request correlation ID.</param>
    /// <param name="version">The API version string. Defaults to "1.0".</param>
    /// <returns>A new <see cref="ResponseMetadata"/> instance.</returns>
    public static ResponseMetadata Create(string correlationId, string version = "1.0") =>
        new(correlationId, DateTime.UtcNow, version);
}
