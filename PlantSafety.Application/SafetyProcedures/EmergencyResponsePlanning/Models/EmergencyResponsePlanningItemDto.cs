// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EmergencyResponsePlanning record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.EmergencyResponsePlanning.Models;

/// <summary>A single EmergencyResponsePlanning record (Emergency response planning procedures ensure employees know how to respond during fires, chemical spills, severe weather, or medical emergencies via documented protocols, evacuation routes, and drills.).</summary>
public sealed record EmergencyResponsePlanningItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EmergencyResponsePlanningId { get; init; }

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
    public required string PlanStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}