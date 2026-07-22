// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single MaterialHandlingStorage record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Models;

/// <summary>A single MaterialHandlingStorage record (Material handling and storage procedures prevent injuries and ensure safe movement of goods: lifting techniques, forklift/pallet-jack/crane rules, and clear walkways/egress.).</summary>
public sealed record MaterialHandlingStorageItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid MaterialHandlingStorageId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Procedure Title.</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Classification: SafeLifting, ForkliftOperation, PalletJackOperation, CraneOperation, MaterialStorage.</summary>
    public required string HandlingCategory { get; init; }

    /// <summary>Storage Location.</summary>
    public required string StorageLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ProcedureStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}