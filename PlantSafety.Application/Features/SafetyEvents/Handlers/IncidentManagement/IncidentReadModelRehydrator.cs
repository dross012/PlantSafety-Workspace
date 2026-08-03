// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Shared helper to rehydrate the Domain IncidentManagement aggregate from an
//   IncidentReadModel row. Extracted from UpdateIncidentCommandHandler's inline Rehydrate call so
//   the new lifecycle handlers (Assign/Escalate/AddNote/Resolve) don't duplicate the same mapping.
// SchemaVersion: 1

using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>Rehydrates the Domain <see cref="IncidentAggregate"/> from an <see cref="IncidentReadModel"/> row.</summary>
public static class IncidentReadModelRehydrator
{
    /// <summary>Reconstructs the aggregate's current persisted state for lifecycle-method invocation.</summary>
    public static IncidentAggregate Rehydrate(IncidentManagementId incidentManagementId, IncidentReadModel currentRow)
        => IncidentAggregate.Rehydrate(
            incidentManagementId: incidentManagementId,
            tenantId: currentRow.TenantId,
            siteId: currentRow.SiteId,
            status: Enum.Parse<IncidentManagementStatus>(currentRow.Status),
            title: currentRow.Title,
            description: currentRow.Description,
            severity: currentRow.Severity,
            location: currentRow.Location,
            incidentOccurredAtUtc: UtcTimestamp.From(currentRow.IncidentOccurredAtUtc.UtcDateTime),
            reportedAtUtc: UtcTimestamp.From(currentRow.ReportedAtUtc.UtcDateTime),
            reportedByWorkerId: UserId.FromGuid(currentRow.ReportedByWorkerId),
            assignedInvestigatorId: currentRow.AssignedInvestigatorId is Guid investigatorId
                ? UserId.FromGuid(investigatorId)
                : null,
            archivedAtUtc: null,
            deletionReason: null,
            createdAtUtc: UtcTimestamp.From(currentRow.CreatedAtUtc.UtcDateTime),
            createdBy: UserId.FromGuid(currentRow.CreatedBy),
            escalationReason: currentRow.EscalationReason,
            escalatedAtUtc: currentRow.EscalatedAtUtc is DateTimeOffset escalatedAt
                ? UtcTimestamp.From(escalatedAt.UtcDateTime)
                : null,
            escalationCount: currentRow.EscalationCount,
            resolutionNotes: currentRow.ResolutionNotes,
            resolvedAtUtc: currentRow.ClosedAtUtc is DateTimeOffset resolvedAt
                ? UtcTimestamp.From(resolvedAt.UtcDateTime)
                : null);
}
