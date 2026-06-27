// ============================================================================
//  File:           AlertErrorCodes.cs
//  Created:        2026-05-26
//  Description:
//      Canonical error codes for Alerts domain exceptions.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Constants;

public static class AlertErrorCodes
{
    public const string NotFound = "ALERT.NOT_FOUND";
    public const string AlreadyExists = "ALERT.ALREADY_EXISTS";
    public const string InvalidTransition = "ALERT.INVALID_TRANSITION";
    public const string InvalidOperation = "ALERT.INVALID_OPERATION";
    public const string AssignmentFailed = "ALERT.ASSIGNMENT_FAILED";
    public const string MetadataError = "ALERT.METADATA_ERROR";
    public const string SlaViolation = "ALERT.SLA_VIOLATION";
    public const string ConcurrencyConflict = "ALERT.CONCURRENCY_CONFLICT";
    public const string ValidationError = "ALERT.VALIDATION_ERROR";
}
