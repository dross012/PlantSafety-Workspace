// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           CreateManagementOfChangeResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateManagementOfChangeCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Result of a successful <see cref="CreateManagementOfChangeCommand"/> execution.</summary>
/// <param name="ManagementOfChangeId">Identity of the newly created ManagementOfChange aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateManagementOfChangeResult(
    Guid ManagementOfChangeId,
    string Status);
