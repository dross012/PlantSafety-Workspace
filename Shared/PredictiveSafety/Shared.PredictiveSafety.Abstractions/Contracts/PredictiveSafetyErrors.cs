// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Contracts
//  File:           PredictiveSafetyErrors.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic Predictive Safety
//                  error codes and the one validation exception it raises.
//                  Derives from ArgumentException, same reasoning
//                  SchedulingErrors.cs (Phase 14) documents: Shared.
//                  ErrorHandling's existing ExceptionClassifier already maps
//                  ArgumentException -> ErrorCategory.Validation -> HTTP 400,
//                  reused rather than reinvented.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Contracts;

/// <summary>Deterministic Predictive Safety error codes.</summary>
public static class PredictiveSafetyErrors
{
    /// <summary>Raised when a predictive evaluation is requested with an empty EventType.</summary>
    public const string MissingEventType = "PREDICTIVE_SAFETY_MISSING_EVENT_TYPE";

    /// <summary>Raised when a PredictiveSafetyContractMetadata carries an unrecognized SchemaVersion.</summary>
    public const string UnsupportedSchemaVersion = "PREDICTIVE_SAFETY_UNSUPPORTED_SCHEMA_VERSION";
}

/// <summary>The one validation exception Predictive Safety raises for a deterministically invalid evaluation request.</summary>
public sealed class PredictiveSafetyValidationException : ArgumentException
{
    /// <summary>The deterministic PredictiveSafetyErrors code identifying the failed validation.</summary>
    public string ErrorCode { get; }

    /// <summary>Constructs a PredictiveSafetyValidationException for <paramref name="errorCode"/>.</summary>
    public PredictiveSafetyValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
