// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Explicit mapping profiles for Incident aggregate → DTOs.
//   No AutoMapper. No reflection. All mappings are hand-written, one-directional.
//   Source type comes from Domain; destination is the Application DTO.
// SchemaVersion: 1

using PlantSafety.Application.Common.Mapping;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;

/// <summary>
/// Maps the Domain <c>Incident</c> read model to <see cref="IncidentDto"/>.
/// The source type is the Infrastructure read model (passed as a dynamic parameter bag
/// from the stored-procedure result set) mapped in Infrastructure before calling this profile.
/// </summary>
/// <remarks>
/// Application mapping is from Infrastructure read model → Application DTO.
/// Domain aggregate → Infrastructure read model mapping lives in Infrastructure.
/// </remarks>
public sealed class IncidentToIncidentDtoMappingProfile
    : IMappingProfile<IncidentReadModel, IncidentDto>
{
    /// <inheritdoc/>
    public IncidentDto Map(IncidentReadModel source)
    {
        return new IncidentDto
        {
            IncidentId = source.IncidentId,
            TenantId = source.TenantId,
            SiteId = source.SiteId,
            Title = source.Title,
            Description = source.Description,
            IncidentOccurredAtUtc = source.IncidentOccurredAtUtc,
            ReportedAtUtc = source.ReportedAtUtc,
            Severity = source.Severity,
            Status = source.Status,
            Location = source.Location,
            ReportedByWorkerId = source.ReportedByWorkerId,
            AssignedInvestigatorId = source.AssignedInvestigatorId,
            ClosedAtUtc = source.ClosedAtUtc,
            LastModifiedByActorId = source.LastModifiedByActorId,
            LastModifiedAtUtc = source.LastModifiedAtUtc,
            ETag = source.ETag,
        };
    }
}

/// <summary>
/// Maps the Domain <c>Incident</c> read model to <see cref="IncidentSummaryDto"/>.
/// </summary>
public sealed class IncidentToIncidentSummaryDtoMappingProfile
    : IMappingProfile<IncidentReadModel, IncidentSummaryDto>
{
    /// <inheritdoc/>
    public IncidentSummaryDto Map(IncidentReadModel source)
    {
        return new IncidentSummaryDto
        {
            IncidentId = source.IncidentId,
            TenantId = source.TenantId,
            Title = source.Title,
            IncidentOccurredAtUtc = source.IncidentOccurredAtUtc,
            Severity = source.Severity,
            Status = source.Status,
            Location = source.Location,
            ReportedByWorkerId = source.ReportedByWorkerId,
        };
    }
}

/// <summary>
/// Infrastructure read model that mirrors the stored-procedure result set columns for Incidents.
/// Defined here as the mapping contract; the Infrastructure layer populates it via Dapper.
/// </summary>
public sealed record IncidentReadModel
{
    /// <summary>Incident identifier.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the incident occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>UTC timestamp when reported.</summary>
    public required DateTimeOffset ReportedAtUtc { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Location description.</summary>
    public required string Location { get; init; }

    /// <summary>Reporting worker.</summary>
    public required Guid ReportedByWorkerId { get; init; }

    /// <summary>Assigned investigator, if any.</summary>
    public Guid? AssignedInvestigatorId { get; init; }

    /// <summary>UTC timestamp when closed, if applicable.</summary>
    public DateTimeOffset? ClosedAtUtc { get; init; }

    /// <summary>Last actor to modify this record.</summary>
    public required Guid LastModifiedByActorId { get; init; }

    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }

    /// <summary>
    /// UTC record-creation timestamp. Added so <see cref="IIncidentReadRepository.GetByIdAsync"/>
    /// callers can <c>IncidentManagement.Rehydrate</c> the Domain aggregate (2026-07-27 Create/Update
    /// handler wiring) without a second round-trip; mirrors [SafetyEvents].[Incident].CreatedAtUtc.
    /// </summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Identity of the user who created this record. Added for the same Rehydrate purpose as
    /// <see cref="CreatedAtUtc"/>; mirrors [SafetyEvents].[Incident].CreatedBy.
    /// </summary>
    public required Guid CreatedBy { get; init; }
}
