// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTaskInvalidStatusTransitionException.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PreventiveMaintenanceTask lifecycle transition is not
//                  permitted. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Exceptions;

/// <summary>Thrown when a PreventiveMaintenanceTask lifecycle transition is not permitted.</summary>
public sealed class PreventiveMaintenanceTaskInvalidStatusTransitionException : PreventiveMaintenanceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PreventiveMaintenanceTaskInvalidStatusTransitionException(
        PreventiveMaintenanceTaskId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition PreventiveMaintenanceTask '{id}' from '{from}' to '{to}'.",
            "PreventiveMaintenanceTask.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
