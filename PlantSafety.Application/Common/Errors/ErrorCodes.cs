// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical error code constants for all Application layer failures.
//   Format: Module.Entity.Condition — never raw strings, never magic strings.
//   Additive-only: new codes may be added; existing codes must not change meaning.
//   Consumed by Result<T>.Failure(), exception constructors, and API ProblemDetails mapping.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added in any class; no existing constants may be removed or renamed.

namespace PlantSafety.Application.Common.Errors;

/// <summary>
/// Canonical error code registry for the Application layer.
/// All error codes follow the <c>Module.Entity.Condition</c> naming convention.
/// No magic strings are permitted; all error code references must use constants from this class.
/// </summary>
public static class ErrorCodes
{
    // ── Cross-cutting ─────────────────────────────────────────────────────────
    /// <summary>Authentication is required but was not provided.</summary>
    public const string Unauthorized = "Platform.Auth.Unauthorized";

    /// <summary>The authenticated user lacks the required permission.</summary>
    public const string Forbidden = "Platform.Auth.Forbidden";

    /// <summary>The request's bound TenantId does not match the server-resolved tenant context (cross-tenant access attempt).</summary>
    public const string TenantMismatch = "Platform.Auth.TenantMismatch";

    /// <summary>One or more input fields failed validation.</summary>
    public const string ValidationFailed = "Platform.Validation.Failed";

    /// <summary>The requested resource was not found.</summary>
    public const string NotFound = "Platform.Resource.NotFound";

    /// <summary>The request conflicts with the current state of the resource.</summary>
    public const string Conflict = "Platform.Resource.Conflict";

    /// <summary>An optimistic concurrency conflict was detected.</summary>
    public const string ConcurrencyConflict = "Platform.Concurrency.Conflict";

    /// <summary>The operation exceeded its allowed execution time.</summary>
    public const string Timeout = "Platform.Operation.Timeout";

    /// <summary>An unexpected internal error occurred.</summary>
    public const string InternalError = "Platform.Internal.Error";

    // ── SafetyEvents (Incident Management) ───────────────────────────────────
    /// <summary>The incident record was not found.</summary>
    public const string IncidentNotFound = "SafetyEvents.Incident.NotFound";

    /// <summary>The incident is in an invalid state for the requested operation.</summary>
    public const string IncidentInvalidState = "SafetyEvents.Incident.InvalidState";

    /// <summary>The corrective action was not found.</summary>
    public const string CorrectiveActionNotFound = "SafetyEvents.CorrectiveAction.NotFound";

    // ── Alerts ────────────────────────────────────────────────────────────────
    /// <summary>The alert record was not found.</summary>
    public const string AlertNotFound = "Alerts.Alert.NotFound";

    /// <summary>The alert escalation rule is invalid or circular.</summary>
    public const string AlertEscalationRuleInvalid = "Alerts.Alert.EscalationRuleInvalid";

    // ── Compliance ────────────────────────────────────────────────────────────
    /// <summary>The compliance obligation was not found.</summary>
    public const string ComplianceObligationNotFound = "Compliance.Obligation.NotFound";

    // ── Permits ───────────────────────────────────────────────────────────────
    /// <summary>The permit record was not found.</summary>
    public const string PermitNotFound = "Permits.Permit.NotFound";

    /// <summary>The permit is in an invalid lifecycle state for the requested transition.</summary>
    public const string PermitInvalidState = "Permits.Permit.InvalidState";

    // ── Financials ────────────────────────────────────────────────────────────
    /// <summary>The budget record was not found.</summary>
    public const string BudgetNotFound = "Financials.Budget.NotFound";

    /// <summary>The budget balance would become negative after this operation.</summary>
    public const string BudgetInsufficientBalance = "Financials.Budget.InsufficientBalance";

    // ── PlanningProjects ──────────────────────────────────────────────────────
    /// <summary>The planning project was not found.</summary>
    public const string PlanningProjectNotFound = "PlanningProjects.Project.NotFound";

    /// <summary>A resource is over-allocated on the project.</summary>
    public const string ProjectResourceOverAllocated = "PlanningProjects.Resource.OverAllocated";

    // ── Training ──────────────────────────────────────────────────────────────
    /// <summary>The training record was not found.</summary>
    public const string TrainingNotFound = "Training.Training.NotFound";

    // ── Inspection ────────────────────────────────────────────────────────────
    /// <summary>The inspection record was not found.</summary>
    public const string InspectionNotFound = "Inspections.Inspection.NotFound";
}

/// <summary>
/// Application-layer authorization exception. Wraps the canonical error envelope.
/// Mapped to HTTP 403 by the API error handler middleware.
/// </summary>
/// <remarks>
/// Severity mapping: <see cref="ErrorSeverity.Authorization"/> → Warning log level.
/// </remarks>
/// <remarks>Initializes a new <see cref="ApplicationAuthorizationException"/>.</remarks>
/// <param name="errorCode">The canonical error code.</param>
/// <param name="message">The sanitized error message.</param>
/// <param name="correlationId">The correlation identifier for lineage tracing.</param>
public sealed class ApplicationAuthorizationException(string errorCode, string message, Guid correlationId) : Exception(message)
{

    /// <summary>Gets the canonical application error descriptor.</summary>
    public ApplicationError Error { get; } = new ApplicationError(errorCode, message, correlationId, ErrorSeverity.Authorization);
}

/// <summary>
/// Application-layer exception for domain rule violations.
/// Mapped to HTTP 400 by the API error handler middleware.
/// </summary>
/// <remarks>Initializes a new <see cref="DomainRuleException"/>.</remarks>
/// <param name="errorCode">The canonical error code.</param>
/// <param name="message">The sanitized error message.</param>
/// <param name="correlationId">The correlation identifier for lineage tracing.</param>
public sealed class DomainRuleException(string errorCode, string message, Guid correlationId) : Exception(message)
{

    /// <summary>Gets the canonical application error descriptor.</summary>
    public ApplicationError Error { get; } = new ApplicationError(errorCode, message, correlationId, ErrorSeverity.Domain);
}

/// <summary>
/// Application-layer exception for optimistic concurrency conflicts.
/// Mapped to HTTP 409 by the API error handler middleware.
/// </summary>
/// <remarks>Initializes a new <see cref="ConcurrencyConflictException"/>.</remarks>
/// <param name="message">The sanitized error message.</param>
/// <param name="correlationId">The correlation identifier for lineage tracing.</param>
public sealed class ConcurrencyConflictException(string message, Guid correlationId) : Exception(message)
{

    /// <summary>Gets the canonical application error descriptor.</summary>
    public ApplicationError Error { get; } = new ApplicationError(ErrorCodes.ConcurrencyConflict, message, correlationId, ErrorSeverity.Domain);
}
