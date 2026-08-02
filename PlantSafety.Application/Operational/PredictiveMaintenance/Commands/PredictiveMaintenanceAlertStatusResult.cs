// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           PredictiveMaintenanceAlertStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the PredictiveMaintenanceAlert lifecycle-transition
//                  commands (Review, ScheduleAction, Resolve, Dismiss, Close). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Result payload shared by PredictiveMaintenanceAlert lifecycle-transition commands.</summary>
/// <param name="PredictiveMaintenanceAlertId">Identity of the affected PredictiveMaintenanceAlert aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PredictiveMaintenanceAlertStatusResult(Guid PredictiveMaintenanceAlertId, string Status);
