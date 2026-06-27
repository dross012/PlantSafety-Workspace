// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side RFC 7807 ProblemDetails representation.
//   Maps the server-side canonical error envelope (§14.4) to a UI-safe DTO
//   that carries full lineage identifiers without leaking infrastructure details.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §14.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Models.Common;

/// <summary>
/// Client-side RFC 7807 Problem Details representation.
/// Includes all lineage fields required for SIEM-safe display and support correlation.
/// </summary>
public sealed class ProblemDetailsDto
{
    /// <summary>Gets the URI reference identifying the problem type.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the short human-readable summary of the problem.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the HTTP status code.</summary>
    public int Status { get; init; }

    /// <summary>Gets the human-readable explanation specific to this occurrence.</summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>Gets the URI reference identifying the specific occurrence of the problem.</summary>
    public string Instance { get; init; } = string.Empty;

    /// <summary>Gets the correlation identifier for support lineage.</summary>
    public Guid CorrelationId { get; init; }

    /// <summary>Gets the causation identifier.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the distributed trace identifier.</summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the error occurrence.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Gets the per-field validation errors. Empty for non-validation problems.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; init; }
        = new Dictionary<string, string[]>();

    /// <summary>
    /// Creates a minimal unknown problem for cases where the API response body
    /// could not be deserialized as a ProblemDetails payload.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <returns>A non-null problem details instance with the given status.</returns>
    public static ProblemDetailsDto Unknown(int statusCode) => new()
    {
        Type = "about:blank",
        Title = "An unexpected error occurred.",
        Status = statusCode,
        Timestamp = DateTimeOffset.UtcNow
    };
}
