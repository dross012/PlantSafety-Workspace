// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single continuous-improvement RCA record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContinuousImprovement.RCA.Models;

/// <summary>A single root cause analysis record tracked by the ContinuousImprovement RCA module.</summary>
public sealed record RCAItemDto
{
    /// <summary>RCA record identifier.</summary>
    public required Guid RcaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Problem statement being analyzed.</summary>
    public required string ProblemStatement { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }

    /// <summary>Current RCA status (e.g., Initiated/InProgress/PendingApproval/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the RCA was initiated.</summary>
    public required DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the RCA.</summary>
    public required Guid OwnerUserId { get; init; }
}
