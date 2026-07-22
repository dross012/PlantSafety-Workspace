// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single LotoProcedures record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.LotoProcedures.Models;

/// <summary>A single LotoProcedures record (Lockout/Tagout procedures protect workers during maintenance by identifying energy sources, applying locks/hasps/tags, verifying zero-energy state, and documenting authorized personnel.).</summary>
public sealed record LotoProceduresItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid LotoProceduresId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Procedure Title.</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Classification: Electrical, Hydraulic, Pneumatic, Thermal, Chemical.</summary>
    public required string EnergySourceType { get; init; }

    /// <summary>Isolation Point Description.</summary>
    public required string IsolationPointDescription { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ProcedureStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}