// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single LoadSecurementCheck record.
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Models;

/// <summary>A single LoadSecurementCheck record.</summary>
public sealed record LoadSecurementCheckItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid LoadSecurementCheckId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the transport trip this check was performed for, if any.</summary>
    public Guid? TripId { get; init; }

    /// <summary>Identity of the vehicle this check was performed on.</summary>
    public required Guid VehicleId { get; init; }

    /// <summary>Overall status classification: Passed, Failed, CorrectiveActionCompleted.</summary>
    public required string CheckStatus { get; init; }

    /// <summary>Count of deficiencies observed during the check.</summary>
    public required int DeficiencyCount { get; init; }

    /// <summary>Securement score (0-100) recorded at check time.</summary>
    public required decimal SecurementScore { get; init; }

    /// <summary>UTC timestamp the check was performed.</summary>
    public required DateTime CheckedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
