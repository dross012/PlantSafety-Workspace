// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Operator record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Operators.Models;

/// <summary>A single machine operator record.</summary>
public sealed record OperatorsItemDto
{
    /// <summary>Operator identifier.</summary>
    public required Guid OperatorId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Operator full name.</summary>
    public required string FullName { get; init; }

    /// <summary>Operator certification level (e.g., Trainee/Certified/Master).</summary>
    public required string CertificationLevel { get; init; }

    /// <summary>Machine currently assigned to the operator, or null when unassigned.</summary>
    public Guid? AssignedMachineId { get; init; }

    /// <summary>Current operator status (e.g., Active/Suspended/Inactive).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the operator's certification was issued.</summary>
    public required DateTime CertifiedAtUtc { get; init; }

    /// <summary>Identity of the user who certified the operator.</summary>
    public required Guid CertifiedBy { get; init; }
}
