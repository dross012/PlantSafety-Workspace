// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EmergencyPreparedness record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.EmergencyPreparedness.Models;

/// <summary>A single EmergencyPreparedness record (Emergency preparedness ensures employees can respond quickly and correctly to fires, spills, severe weather, or medical emergencies via evacuation routes, e-stop devices, drills, and alarm testing.).</summary>
public sealed record EmergencyPreparednessItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EmergencyPreparednessId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Plan Title.</summary>
    public required string PlanTitle { get; init; }

    /// <summary>Classification: Fire, ChemicalSpill, SevereWeather, MedicalEmergency.</summary>
    public required string EmergencyType { get; init; }

    /// <summary>Evacuation Route.</summary>
    public required string EvacuationRoute { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string PreparednessStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}