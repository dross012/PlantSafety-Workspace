// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           EmergencyDrillPlanningStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the EmergencyDrillPlanning lifecycle-transition
//                  commands (Conduct, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Result payload shared by EmergencyDrillPlanning lifecycle-transition commands.</summary>
/// <param name="EmergencyDrillPlanningId">Identity of the affected EmergencyDrillPlanning aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record EmergencyDrillPlanningStatusResult(Guid EmergencyDrillPlanningId, string Status);
