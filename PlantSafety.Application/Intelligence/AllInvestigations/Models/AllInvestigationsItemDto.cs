// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single AI-assisted safety investigation record.
// SchemaVersion: 1
namespace PlantSafety.Application.Intelligence.AllInvestigations.Models;

/// <summary>Represents a single AI-assisted safety investigation record.</summary>
public sealed record AllInvestigationsItemDto
{
    /// <summary>Unique identifier of the AI-assisted investigation record.</summary>
    public required Guid InvestigationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reference identifier of the related safety incident.</summary>
    public required string IncidentReference { get; init; }
    /// <summary>AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public required double AiConfidenceScore { get; init; }
    /// <summary>AI-generated summary of the investigation.</summary>
    public string? Summary { get; init; }

    /// <summary>Current investigation status (e.g., Open/InProgress/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the investigation was opened.</summary>
    public required DateTime OpenedAtUtc { get; init; }

    /// <summary>Identity of the user or process assigned to the investigation.</summary>
    public required Guid AssignedTo { get; init; }
}