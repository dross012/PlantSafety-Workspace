// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single EgressSimulationRun record
//                    ("DigitalTwin.EvacuationModeling.EgressSimulations").
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EgressSimulations.Models;

/// <summary>A single EgressSimulationRun record.</summary>
public sealed record EgressSimulationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EgressSimulationRunId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the evacuation zone this run models.</summary>
    public required Guid ZoneId { get; init; }

    /// <summary>Version of the Digital Twin model this run's geometry was sourced from.</summary>
    public required string TwinModelVersion { get; init; }

    /// <summary>Reason this run was triggered: Manual, TwinModelChangeResimulation, QuarterlyFullSite, DrillTriggeredReplay.</summary>
    public required string TriggerReason { get; init; }

    /// <summary>Overall status classification: Running, Completed, Failed.</summary>
    public required string SimulationStatus { get; init; }

    /// <summary>Simulated total evacuation time in seconds, populated once completed.</summary>
    public int? SimulatedEvacuationTimeSeconds { get; init; }

    /// <summary>Predicted peak occupancy considered by the simulation, if supplied.</summary>
    public int? PredictedPeakOccupancy { get; init; }

    /// <summary>Identity of the drill (SafetyEvents.DrillsAndExercises) this run is correlated with, if any.</summary>
    public Guid? LinkedDrillId { get; init; }

    /// <summary>UTC timestamp the run started.</summary>
    public required DateTime StartedAtUtc { get; init; }

    /// <summary>UTC timestamp the run completed or failed, if it has.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
