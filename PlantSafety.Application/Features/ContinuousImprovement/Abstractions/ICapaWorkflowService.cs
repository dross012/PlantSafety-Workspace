// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the ContinuousImprovement module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Abstractions;

/// <summary>Manages CAPA lifecycle transitions and escalation rules.</summary>
public interface ICapaWorkflowService
{
    /// <summary>Returns CAPA identifiers that are overdue based on DueAtUtc.</summary>
    Task<IReadOnlyList<Guid>> GetOverdueCaIdsAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken);

    /// <summary>Evaluates whether a CAPA can transition from its current status to the target status.</summary>
    bool CanTransition(string currentStatus, string targetStatus);
}
