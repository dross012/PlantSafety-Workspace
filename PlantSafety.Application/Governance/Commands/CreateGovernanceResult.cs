// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           CreateGovernanceResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateGovernanceCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Governance.Commands;

/// <summary>Result of a successful <see cref="CreateGovernanceCommand"/> execution.</summary>
public sealed record CreateGovernanceResult(
    Guid GovernanceId,
    string Status);
