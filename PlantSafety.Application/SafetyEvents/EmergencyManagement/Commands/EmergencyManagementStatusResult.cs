// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           EmergencyManagementStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the EmergencyManagement lifecycle-transition
//                  commands (RespondTo, Stabilize, Resolve, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;

/// <summary>Result payload shared by EmergencyManagement lifecycle-transition commands.</summary>
/// <param name="EmergencyManagementId">Identity of the affected EmergencyManagement aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record EmergencyManagementStatusResult(Guid EmergencyManagementId, string Status);
