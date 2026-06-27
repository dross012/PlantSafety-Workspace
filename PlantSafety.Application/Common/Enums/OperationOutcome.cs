// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical operation outcome enum for structured log envelopes and command results.
//   Used in OperationLogEnvelope.Success and as a success detail discriminator in Result types.
//   Simple enum (no behavior, no metadata) — Smart Enum not warranted per §2.7.
// SchemaVersion: 1
// Additive-only evolution: new values may be added; existing values must not be renamed or removed.

namespace PlantSafety.Application.Common.Enums;

/// <summary>
/// Canonical classification of operation outcomes for structured logging and audit trails.
/// Used in every <see cref="Logging.OperationLogEnvelope"/> and domain event attribution.
/// </summary>
public enum OperationOutcome
{
    /// <summary>The operation completed successfully.</summary>
    Success = 0,

    /// <summary>The operation failed due to a validation error.</summary>
    ValidationFailure = 1,

    /// <summary>The operation failed due to an authorization denial.</summary>
    AuthorizationFailure = 2,

    /// <summary>The operation failed because the target resource was not found.</summary>
    NotFound = 3,

    /// <summary>The operation failed due to a domain rule violation.</summary>
    DomainFailure = 4,

    /// <summary>The operation failed due to an optimistic concurrency conflict.</summary>
    ConcurrencyConflict = 5,

    /// <summary>The operation failed due to an internal infrastructure error.</summary>
    InfrastructureFailure = 6,

    /// <summary>The operation was cancelled by the caller.</summary>
    Cancelled = 7
}
