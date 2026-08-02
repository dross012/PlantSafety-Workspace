// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           CrisisManagementStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the CrisisManagement lifecycle-transition commands
//                  (RespondTo, Stabilize, Resolve, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;

/// <summary>Result payload shared by CrisisManagement lifecycle-transition commands.</summary>
/// <param name="CrisisManagementId">Identity of the affected CrisisManagement aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record CrisisManagementStatusResult(Guid CrisisManagementId, string Status);
