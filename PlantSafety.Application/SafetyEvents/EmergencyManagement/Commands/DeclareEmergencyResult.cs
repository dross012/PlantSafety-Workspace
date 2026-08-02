// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           DeclareEmergencyResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for DeclareEmergencyCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;

/// <summary>Result of a successful <see cref="DeclareEmergencyCommand"/> execution.</summary>
/// <param name="EmergencyManagementId">Identity of the newly declared EmergencyManagement aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record DeclareEmergencyResult(
    Guid EmergencyManagementId,
    string Status);
