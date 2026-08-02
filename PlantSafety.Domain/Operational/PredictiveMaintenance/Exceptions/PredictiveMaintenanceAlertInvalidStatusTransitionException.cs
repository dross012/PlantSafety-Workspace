// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlertInvalidStatusTransitionException.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PredictiveMaintenanceAlert lifecycle transition is not
//                  permitted. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Exceptions;

/// <summary>Thrown when a PredictiveMaintenanceAlert lifecycle transition is not permitted.</summary>
public sealed class PredictiveMaintenanceAlertInvalidStatusTransitionException : PredictiveMaintenanceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PredictiveMaintenanceAlertInvalidStatusTransitionException(
        PredictiveMaintenanceAlertId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition PredictiveMaintenanceAlert '{id}' from '{from}' to '{to}'.",
            "PredictiveMaintenanceAlert.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
