// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the nine Regulatory Calendar
//   read-model endpoints (Deadlines: AnnualAudit/EPA; Inspections: DOT/
//   EnvironmentalSampling/Equipment/OSHA; Renewals: DOT/ISO/Training). Each
//   sub-type shares the same generic envelope shape (TenantId/SiteId/
//   GeneratedAt/Items) but a distinct item schema, so a single generic
//   response wrapper (RegulatoryCalendarResponseDto&lt;TItem&gt;) serves all
//   nine. Deserialized directly into the inner Data shape of the
//   ApiResponse&lt;T&gt; envelope, matching the established sibling convention
//   (see Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.Models;

/// <summary>Generic response payload shared by every Regulatory Calendar read-model list endpoint.</summary>
/// <typeparam name="TItem">The endpoint-specific item DTO type.</typeparam>
public sealed class RegulatoryCalendarResponseDto<TItem>
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching items. Never null; empty when no results.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}

/// <summary>Deadlines/AnnualAudit item (GET v1/compliance/regulatory-calendar/deadlines/annual-audit).</summary>
public sealed class AnnualAuditItemDto
{
    /// <summary>Deadline identifier.</summary>
    public Guid DeadlineId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Audit scope description.</summary>
    public string AuditScope { get; init; } = string.Empty;
    /// <summary>UTC timestamp the deadline is due.</summary>
    public DateTime DueAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the responsible party.</summary>
    public Guid ResponsibleParty { get; init; }
}

/// <summary>Deadlines/EPA item (GET v1/compliance/regulatory-calendar/deadlines/epa).</summary>
public sealed class EpaDeadlineItemDto
{
    /// <summary>Deadline identifier.</summary>
    public Guid DeadlineId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>EPA regulation reference.</summary>
    public string EPARegulation { get; init; } = string.Empty;
    /// <summary>UTC timestamp the deadline is due.</summary>
    public DateTime DueAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the responsible party.</summary>
    public Guid ResponsibleParty { get; init; }
}

/// <summary>Inspections/DOT item (GET v1/compliance/regulatory-calendar/inspections/dot).</summary>
public sealed class DotInspectionItemDto
{
    /// <summary>Inspection identifier.</summary>
    public Guid InspectionId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Inspection scope description.</summary>
    public string InspectionScope { get; init; } = string.Empty;
    /// <summary>UTC timestamp the inspection is scheduled.</summary>
    public DateTime ScheduledAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the assigned inspector.</summary>
    public Guid Inspector { get; init; }
}

/// <summary>Inspections/EnvironmentalSampling item (GET v1/compliance/regulatory-calendar/inspections/environmental-sampling).</summary>
public sealed class EnvironmentalSamplingItemDto
{
    /// <summary>Inspection identifier.</summary>
    public Guid InspectionId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Sample type description.</summary>
    public string SampleType { get; init; } = string.Empty;
    /// <summary>UTC timestamp the sampling is scheduled.</summary>
    public DateTime ScheduledAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the assigned inspector.</summary>
    public Guid Inspector { get; init; }
}

/// <summary>Inspections/Equipment item (GET v1/compliance/regulatory-calendar/inspections/equipment).</summary>
public sealed class EquipmentInspectionItemDto
{
    /// <summary>Inspection identifier.</summary>
    public Guid InspectionId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Equipment category description.</summary>
    public string EquipmentCategory { get; init; } = string.Empty;
    /// <summary>UTC timestamp the inspection is scheduled.</summary>
    public DateTime ScheduledAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the assigned inspector.</summary>
    public Guid Inspector { get; init; }
}

/// <summary>Inspections/OSHA item (GET v1/compliance/regulatory-calendar/inspections/osha).</summary>
public sealed class OshaInspectionItemDto
{
    /// <summary>Inspection identifier.</summary>
    public Guid InspectionId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Inspection scope description.</summary>
    public string InspectionScope { get; init; } = string.Empty;
    /// <summary>UTC timestamp the inspection is scheduled.</summary>
    public DateTime ScheduledAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the assigned inspector.</summary>
    public Guid Inspector { get; init; }
}

/// <summary>Renewals/DOT item (GET v1/compliance/regulatory-calendar/renewals/dot).</summary>
public sealed class DotRenewalItemDto
{
    /// <summary>Renewal identifier.</summary>
    public Guid RenewalId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Credential type description.</summary>
    public string CredentialType { get; init; } = string.Empty;
    /// <summary>UTC timestamp the credential expires.</summary>
    public DateTime ExpiresAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the responsible party.</summary>
    public Guid ResponsibleParty { get; init; }
}

/// <summary>Renewals/ISO item (GET v1/compliance/regulatory-calendar/renewals/iso).</summary>
public sealed class IsoRenewalItemDto
{
    /// <summary>Renewal identifier.</summary>
    public Guid RenewalId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Certification standard description.</summary>
    public string CertificationStandard { get; init; } = string.Empty;
    /// <summary>UTC timestamp the certification expires.</summary>
    public DateTime ExpiresAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the responsible party.</summary>
    public Guid ResponsibleParty { get; init; }
}

/// <summary>Renewals/Training item (GET v1/compliance/regulatory-calendar/renewals/training).</summary>
public sealed class TrainingRenewalItemDto
{
    /// <summary>Renewal identifier.</summary>
    public Guid RenewalId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }
    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
    /// <summary>Training program description.</summary>
    public string TrainingProgram { get; init; } = string.Empty;
    /// <summary>UTC timestamp the training expires.</summary>
    public DateTime ExpiresAtUtc { get; init; }
    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Identity of the responsible party.</summary>
    public Guid ResponsibleParty { get; init; }
}
