// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the EmergencyResponsePlanning aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EmergencyResponsePlanning"/> aggregate.</summary>
public static class EmergencyResponsePlanningLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the plan may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(EmergencyResponsePlanningStatus current) => current == EmergencyResponsePlanningStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the plan may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(EmergencyResponsePlanningStatus current) => current == EmergencyResponsePlanningStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when a drill may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleDrill(EmergencyResponsePlanningStatus current) => current == EmergencyResponsePlanningStatus.Published;

    /// <summary>Returns <see langword="true"/> when a drill outcome may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordDrill(EmergencyResponsePlanningStatus current) => current == EmergencyResponsePlanningStatus.DrillScheduled;
}
