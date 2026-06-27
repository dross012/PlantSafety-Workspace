// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical application error type for all failure result values.
//   ApplicationError is the ONLY permitted error type for Result<T>.Failure() values.
//   Never use raw exceptions as result values. Never expose infrastructure details in the error message.
//   All errors include CorrelationId for lineage tracing to the SIEM pipeline.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added; no existing fields may be removed or renamed.

namespace PlantSafety.Application.Common.Errors;

/// <summary>
/// Immutable canonical error descriptor for Application layer failures.
/// Used as the failure value in <see cref="Results.Result{T}"/>.
/// Never contains PII, stack traces, SQL text, or connection strings.
/// </summary>
public sealed record ApplicationError
{
    /// <summary>Initializes a new <see cref="ApplicationError"/>.</summary>
    /// <param name="errorCode">The canonical error code (e.g., <c>SafetyEvents.Incidents.NotFound</c>).</param>
    /// <param name="message">The sanitized, user-safe error message.</param>
    /// <param name="correlationId">The correlation identifier for lineage tracing.</param>
    /// <param name="severity">The severity classification for log routing.</param>
    public ApplicationError(
        string errorCode,
        string message,
        Guid correlationId,
        ErrorSeverity severity = ErrorSeverity.Error)
    {
        ErrorCode = errorCode;
        Message = message;
        CorrelationId = correlationId;
        Severity = severity;
        TimestampUtc = DateTime.UtcNow;
    }

    /// <summary>Gets the canonical error code in <c>Module.Entity.Condition</c> format.</summary>
    public string ErrorCode { get; }

    /// <summary>Gets the sanitized, user-safe error message. Never contains PII or infrastructure details.</summary>
    public string Message { get; }

    /// <summary>Gets the correlation identifier for lineage tracing in the SIEM pipeline.</summary>
    public Guid CorrelationId { get; }

    /// <summary>Gets the severity classification for log routing and alerting.</summary>
    public ErrorSeverity Severity { get; }

    /// <summary>Gets the UTC timestamp when the error occurred.</summary>
    public DateTime TimestampUtc { get; }

    /// <summary>Gets optional validation failures for <see cref="ErrorCodes.ValidationFailed"/> errors.</summary>
    public IReadOnlyList<ValidationFailureDetail>? ValidationFailures { get; init; }

    /// <inheritdoc />
    public override string ToString() => $"[{ErrorCode}] {Message} (CorrelationId={CorrelationId})";
}

/// <summary>Canonical severity classification for error routing and alerting.</summary>
public enum ErrorSeverity
{
    /// <summary>Validation failure; low severity; mapped to HTTP 422.</summary>
    Validation = 0,

    /// <summary>Authorization failure; medium severity; mapped to HTTP 403.</summary>
    Authorization = 1,

    /// <summary>Not found; low severity; mapped to HTTP 404.</summary>
    NotFound = 2,

    /// <summary>Domain rule violation; high severity; mapped to HTTP 400.</summary>
    Domain = 3,

    /// <summary>Application-layer failure; high severity; mapped to HTTP 500.</summary>
    Error = 4,

    /// <summary>Critical platform failure; requires immediate escalation.</summary>
    Critical = 5
}

/// <summary>Detail record for individual validation failures within a ValidationFailed error.</summary>
public sealed record ValidationFailureDetail
{
    /// <summary>Gets the field/property name that failed validation.</summary>
    public required string Field { get; init; }

    /// <summary>Gets the sanitized validation error message.</summary>
    public required string Message { get; init; }

    /// <summary>Gets the validation error code for programmatic handling.</summary>
    public string? Code { get; init; }
}
