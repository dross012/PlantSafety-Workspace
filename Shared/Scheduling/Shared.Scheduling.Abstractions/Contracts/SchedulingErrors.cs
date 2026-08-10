// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Contracts
//  File:           SchedulingErrors.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable.
//                  Deterministic Scheduling error codes and the one
//                  validation exception type Scheduling raises. Deliberately
//                  derives from ArgumentException rather than inventing a
//                  new exception hierarchy: Shared.ErrorHandling's existing
//                  ExceptionClassifier (Phase 2.3) already maps
//                  ArgumentException to ErrorCategory.Validation, and
//                  ErrorCategoryHttpMapping already maps that to HTTP 400 --
//                  reusing this means a Scheduling validation failure gets a
//                  correctly classified 400 through the existing
//                  GlobalErrorHandler pipeline with zero new error-handling
//                  code, exactly the reasoning WorkflowEngineAuthorization.cs
//                  documents for reusing UnauthorizedAccessException.
// ============================================================================
namespace Shared.Scheduling.Abstractions.Contracts;

/// <summary>Deterministic Scheduling error codes, referenced by SchedulingValidationException.ErrorCode and governance violation reports.</summary>
public static class SchedulingErrors
{
    /// <summary>Raised when a scheduling evaluation is requested with zero RequiredActions.</summary>
    public const string NoRequiredActionsSupplied = "SCHEDULING_NO_REQUIRED_ACTIONS_SUPPLIED";

    /// <summary>Raised when a RequiredAction's DueWithinHours is negative.</summary>
    public const string NegativeDueWithinHours = "SCHEDULING_NEGATIVE_DUE_WITHIN_HOURS";

    /// <summary>Raised when two RequiredActions in the same evaluation share an ActionCode.</summary>
    public const string DuplicateActionCode = "SCHEDULING_DUPLICATE_ACTION_CODE";

    /// <summary>Raised when a SchedulingContractMetadata carries an unrecognized SchemaVersion.</summary>
    public const string UnsupportedSchemaVersion = "SCHEDULING_UNSUPPORTED_SCHEMA_VERSION";
}

/// <summary>The one validation exception Scheduling raises for a deterministically invalid evaluation request.</summary>
public sealed class SchedulingValidationException : ArgumentException
{
    /// <summary>The deterministic SchedulingErrors code identifying the failed validation.</summary>
    public string ErrorCode { get; }

    /// <summary>Constructs a SchedulingValidationException for <paramref name="errorCode"/>.</summary>
    public SchedulingValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
