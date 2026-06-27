// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the Compliance module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Abstractions;

/// <summary>Manages regulatory obligation due-date calculations.</summary>
public interface IComplianceCalendarService
{
    /// <summary>Returns the next due date for the given regulatory obligation recurrence rule.</summary>
    Task<DateTimeOffset?> GetNextDueDateAsync(
        Guid tenantId,
        Guid obligationId,
        CancellationToken cancellationToken);

    /// <summary>Returns all obligations overdue as of the given UTC timestamp.</summary>
    Task<IReadOnlyList<Guid>> GetOverdueObligationIdsAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken);
}

/// <summary>Validates that a permit's validity window does not conflict with existing active permits.</summary>
public interface IPermitConflictService
{
    /// <summary>
    /// Returns <c>true</c> when no conflicting active permit exists for the same site, type and time window.
    /// </summary>
    Task<bool> IsWindowAvailableAsync(
        Guid tenantId,
        Guid siteId,
        string permitType,
        DateTimeOffset validFromUtc,
        DateTimeOffset validToUtc,
        Guid? excludePermitId,
        CancellationToken cancellationToken);
}
