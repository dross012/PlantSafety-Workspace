// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: SIEM-safe structured logging abstraction for the Application layer.
//   All logs MUST use canonical fields from §XX (Canonical Observability Field Dictionary).
//   Never log PII, secrets, stack traces, SQL text, or connection strings.
//   One structured envelope per log call — no concatenated strings, no message templates.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Logging;

/// <summary>
/// Provides SIEM-safe structured logging for Application layer handlers and behaviors.
/// All log emissions MUST produce a single structured object per call using canonical field names.
/// The Serilog infrastructure implementation routes to Azure Monitor / Application Insights.
/// </summary>
public interface IApplicationLogger
{
    /// <summary>
    /// Emits a structured Information log for a successfully completed operation.
    /// Success logs mirror the catch pattern envelope (minus exception fields).
    /// </summary>
    /// <param name="envelope">The canonical success log envelope.</param>
    void LogSuccess(OperationLogEnvelope envelope);

    /// <summary>
    /// Emits a structured Warning log for a recoverable condition (e.g., retry, degraded mode).
    /// </summary>
    /// <param name="envelope">The canonical warning log envelope.</param>
    void LogWarning(OperationLogEnvelope envelope);

    /// <summary>
    /// Emits a structured Error log for an operation failure.
    /// Must include sanitized exception context; never raw stack traces.
    /// </summary>
    /// <param name="envelope">The canonical error log envelope with sanitized exception fields.</param>
    void LogError(OperationLogEnvelope envelope);

    /// <summary>
    /// Emits a structured Debug log. Disabled in production/CI builds.
    /// </summary>
    /// <param name="envelope">The canonical debug log envelope.</param>
    void LogDebug(OperationLogEnvelope envelope);
}

/// <summary>
/// Canonical structured log envelope with all required SIEM-safe fields.
/// One instance is emitted per log call — never concatenated strings.
/// </summary>
public sealed record OperationLogEnvelope
{
    /// <summary>
    /// Gets the log schema version (Handbook §2.33 Canonical Log Schema Versioning Rule).
    /// MUST be a string, MUST appear first, and MUST evolve additively.
    /// </summary>
    public string SchemaVersion { get; init; } = "2026.1";

    /// <summary>Gets the operation name in canonical <c>Module.Operation</c> format.</summary>
    public required string OperationName { get; init; }

    /// <summary>Gets the MediatR handler type name for handler-level traceability.</summary>
    public string? HandlerType { get; init; }

    /// <summary>Gets the correlation identifier.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Gets the causation identifier.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the inbound request identifier.</summary>
    public Guid? RequestId { get; init; }

    /// <summary>Gets the OpenTelemetry trace identifier.</summary>
    public string? TraceId { get; init; }

    /// <summary>Gets the OpenTelemetry span identifier.</summary>
    public string? SpanId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the actor identifier.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>Gets the UTC timestamp of the log emission.</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Gets the elapsed milliseconds for the operation.</summary>
    public long ElapsedMs { get; init; }

    /// <summary>Gets whether the operation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Gets the sanitized exception type name. Null on success.</summary>
    public string? ExceptionType { get; init; }

    /// <summary>Gets the sanitized exception message. Never raw stack trace.</summary>
    public string? Message { get; init; }

    /// <summary>Gets the HRESULT of the exception. Zero on success.</summary>
    public int HResult { get; init; }

    /// <summary>Gets the stable hash of the stack trace (never the full stack trace).</summary>
    public string? StackTraceHash { get; init; }

    /// <summary>Gets the log severity classification.</summary>
    public required LogSeverity Severity { get; init; }

    /// <summary>Gets the service name for multi-service log correlation.</summary>
    public string ServiceName { get; init; } = "PlantSafety.Application";

    /// <summary>Gets the deployment environment.</summary>
    public string? Environment { get; init; }
}

/// <summary>Canonical severity levels for structured log classification.</summary>
public enum LogSeverity
{
    /// <summary>Successful operation or expected state transition.</summary>
    Information = 0,

    /// <summary>Recoverable condition; retries may resolve the issue.</summary>
    Warning = 1,

    /// <summary>Non-recoverable failure; requires investigation.</summary>
    Error = 2,

    /// <summary>Platform-level or unhandled failure; requires immediate escalation.</summary>
    Critical = 3,

    /// <summary>Development-only diagnostic information; disabled in production.</summary>
    Debug = 4
}
