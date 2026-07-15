// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side API response envelope that wraps every API
//   response into a discriminated Success / Problem / Fault shape.
//   Prevents null reference and exception leakage into components.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §2.11
// ============================================================

using System;

using PlantSafety.UI.Models.Common;

namespace PlantSafety.UI.Models.Common;

/// <summary>
/// Discriminated envelope for all API responses. Consumers MUST check
/// <see cref="IsSuccess"/> before accessing <see cref="Data"/>.
/// </summary>
/// <typeparam name="T">The expected response payload type.</typeparam>
public sealed class ApiResponseEnvelope<T>
{
    /// <summary>Gets a value indicating whether the request completed successfully.</summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// Gets the deserialized response payload. Only valid when <see cref="IsSuccess"/> is true.
    /// </summary>
    public T? Data { get; private init; }

    /// <summary>
    /// Gets the RFC 7807 problem details when the API returned a non-success status.
    /// Only populated when <see cref="IsSuccess"/> is false and the response was an API error.
    /// </summary>
    public ProblemDetailsDto? Problem { get; private init; }

    /// <summary>
    /// Gets the exception type name when a network or deserialization fault occurred.
    /// Only populated when <see cref="IsSuccess"/> is false and no API response was received.
    /// </summary>
    public string? FaultType { get; private init; }

    /// <summary>Gets a sanitized fault message. Never contains PII, SQL, or stack traces.</summary>
    public string? FaultMessage { get; private init; }

    /// <summary>Gets a value indicating whether this is a fault (network/deserialization failure).</summary>
    public bool IsFault => !IsSuccess && FaultType is not null;

    /// <summary>Gets a value indicating whether this is an API-level problem (non-2xx HTTP).</summary>
    public bool IsProblem => !IsSuccess && Problem is not null;

    /// <summary>Creates a success envelope with the deserialized payload.</summary>
    /// <param name="data">The deserialized API response.</param>
    public static ApiResponseEnvelope<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>Creates a problem envelope from an RFC 7807 ProblemDetails response.</summary>
    /// <param name="problem">The parsed problem details.</param>
    public static ApiResponseEnvelope<T> FromProblem(ProblemDetailsDto problem) => new()
    {
        IsSuccess = false,
        Problem = problem
    };

    /// <summary>Creates a fault envelope from a caught exception (network, timeout, parse).</summary>
    /// <param name="exceptionType">The exception type name. Never the full stack trace.</param>
    /// <param name="sanitizedMessage">The sanitized exception message, free of PII and SQL.</param>
    public static ApiResponseEnvelope<T> Fault(string exceptionType, string sanitizedMessage) => new()
    {
        IsSuccess = false,
        FaultType = exceptionType,
        FaultMessage = sanitizedMessage
    };
}
