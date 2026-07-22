// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the SafetyMeetingParticipation read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.SafetyMeetingParticipation.Repositories;

/// <summary>Read-only repository for SafetyMeetingParticipation data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyMeetingParticipationRepository
{
    /// <summary>Returns SafetyMeetingParticipation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyMeetingParticipationItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}