// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single ActiveMonitoringAuditing record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Models;

/// <summary>A single ActiveMonitoringAuditing record (Active monitoring and auditing provides continuous oversight of contractors once work begins: site visits, PPE compliance verification, equipment/LOTO inspection, and permit-condition adherence.).</summary>
public sealed record ActiveMonitoringAuditingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ActiveMonitoringAuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Monitoring Title.</summary>
    public required string MonitoringTitle { get; init; }

    /// <summary>Classification: SiteVisit, PpeCompliance, EquipmentInspection, PermitConditionAdherence.</summary>
    public required string MonitoringCategory { get; init; }

    /// <summary>Work Location.</summary>
    public required string WorkLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string MonitoringStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}