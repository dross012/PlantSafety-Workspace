// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical RFC 7807 error envelope for all API error responses.
//   §2.11 (API Layer Rule): all errors MUST return ProblemDetails with CorrelationId, CausationId, RequestId.
//   §14.4 (Canonical Error Envelope): required fields defined.
//   Never expose domain entities, stack traces, SQL errors, or infrastructure details.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added; existing fields must not be removed or renamed.

namespace PlantSafety.Application.Contracts.Responses;

/// <summary>
/// Canonical RFC 7807 error response envelope.
/// All API error responses MUST serialize to this shape — never raw exception messages.
/// Consumed by the API layer middleware that maps Application exceptions to HTTP responses.
/// </summary>
public sealed record CanonicalErrorEnvelope
{
    /// <summary>Gets the RFC 7807 type URI (error category).</summary>
    public required string Type { get; init; }

    /// <summary>Gets the human-readable title of the error category.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the HTTP status code.</summary>
    public required int Status { get; init; }

    /// <summary>Gets the sanitized, user-safe detail message. Never contains PII or stack traces.</summary>
    public required string Detail { get; init; }

    /// <summary>Gets the relative URI identifying the specific occurrence of the error.</summary>
    public string? Instance { get; init; }

    /// <summary>Gets the correlation identifier for lineage tracing to the SIEM pipeline.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Gets the causation identifier that identifies the parent command/event.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the inbound request identifier.</summary>
    public Guid? RequestId { get; init; }

    /// <summary>Gets the UTC timestamp when the error occurred.</summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>Gets the canonical error code in Module.Entity.Condition format.</summary>
    public required string ErrorCode { get; init; }

    /// <summary>Gets optional per-field validation errors for validation failures.</summary>
    public IReadOnlyList<FieldValidationError>? Errors { get; init; }
}

/// <summary>
/// Per-field validation error within a <see cref="CanonicalErrorEnvelope"/>.
/// </summary>
public sealed record FieldValidationError
{
    /// <summary>Gets the field/property name that failed validation.</summary>
    public required string Field { get; init; }

    /// <summary>Gets the sanitized validation error message for this field.</summary>
    public required string Message { get; init; }

    /// <summary>Gets the validation error code for programmatic handling.</summary>
    public string? Code { get; init; }
}
