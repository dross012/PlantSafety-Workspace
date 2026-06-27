/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ErrorType.cs
    DESCRIPTION:        Canonical error type enumeration. Represents the category of an error. MUST remain stable and
                        additive-only.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New values MAY be added; existing values MUST NOT change semantics.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Errors;

/// <summary>
/// Canonical error type categories.
/// </summary>
public enum ErrorType
{
    Validation = 0,
    NotFound = 1,
    Conflict = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Failure = 5,
    Timeout = 6,
    DependencyFailure = 7,
    RateLimited = 8,
    BadRequest = 9,
    BusinessRuleViolation = 10,
    StateViolation = 11,
    DataIntegrity = 12,
    IOFailure = 13,
    OperationCanceled = 14,
    UnhandledException = 15,
    Configuration = 16,
    DependencyUnavailable = 17,
    VersionMismatch = 18,
    QuotaExceeded = 19,
    ServiceUnavailable = 20,
    Concurrency = 21,
    PreconditionFailed = 22,
}
