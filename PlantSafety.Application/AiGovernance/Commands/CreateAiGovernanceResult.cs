// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           CreateAiGovernanceResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateAiGovernanceCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiGovernance.Commands;

/// <summary>Result of a successful <see cref="CreateAiGovernanceCommand"/> execution.</summary>
public sealed record CreateAiGovernanceResult(
    Guid AiGovernanceId,
    string Status);
