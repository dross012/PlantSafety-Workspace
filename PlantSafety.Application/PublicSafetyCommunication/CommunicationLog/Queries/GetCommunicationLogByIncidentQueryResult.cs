// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCommunicationLogByIncidentQuery.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;

/// <summary>Result of <see cref="GetCommunicationLogByIncidentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="IncidentId">Incident the data is scoped to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CommunicationLogEntry items, ordered chronologically.</param>
public sealed record GetCommunicationLogByIncidentQueryResult(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<CommunicationLogEntryItemDto> Items);
