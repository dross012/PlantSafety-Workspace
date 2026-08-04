// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           LocalEmergencyCoordinationStatusResult.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the LocalEmergencyCoordination case-level
//                  lifecycle-transition commands (Close, Archive). Mirrors EmergencyManagementStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Result payload shared by LocalEmergencyCoordination case-level lifecycle-transition commands.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record LocalEmergencyCoordinationStatusResult(Guid LocalEmergencyCoordinationId, string Status);
