// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           JointResponsePlanStatusResult.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the joint-response-plan lifecycle commands
//                  (Activate/Deactivate). Mirrors EmergencyManagementStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;

/// <summary>Result payload shared by joint-response-plan lifecycle-transition commands.</summary>
/// <param name="LocalEmergencyCoordinationId">Identity of the affected LocalEmergencyCoordination case.</param>
/// <param name="JointResponsePlanId">Identity of the affected joint response plan.</param>
/// <param name="IsActive">Whether the plan is active after the transition.</param>
public sealed record JointResponsePlanStatusResult(Guid LocalEmergencyCoordinationId, Guid JointResponsePlanId, bool IsActive);
