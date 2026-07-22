// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single LotoPrograms record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.LotoPrograms.Models;

/// <summary>A single LotoPrograms record (Lockout/Tagout (LOTO) programs protect employees during maintenance, repair, or cleaning by ensuring machines remain fully de-energized through identification of energy sources, isolation devices, and verification steps.).</summary>
public sealed record LotoProgramsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid LotoProgramsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Program Title.</summary>
    public required string ProgramTitle { get; init; }

    /// <summary>Classification: Electrical, Hydraulic, Pneumatic, Thermal, Chemical.</summary>
    public required string EnergySourceType { get; init; }

    /// <summary>Isolation Point Description.</summary>
    public required string IsolationPointDescription { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ProgramStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}