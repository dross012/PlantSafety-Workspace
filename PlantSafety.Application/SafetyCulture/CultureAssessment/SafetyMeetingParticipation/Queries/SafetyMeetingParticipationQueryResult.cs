// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSafetyMeetingParticipationQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Queries;

/// <summary>Result of <see cref="GetSafetyMeetingParticipationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching SafetyMeetingParticipation items.</param>
public sealed record SafetyMeetingParticipationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SafetyMeetingParticipationItemDto> Items);