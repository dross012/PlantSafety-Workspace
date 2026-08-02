// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CreateAiSafetyIntelligenceResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateAiSafetyIntelligenceCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiSafetyIntelligence.Commands;

/// <summary>Result of a successful <see cref="CreateAiSafetyIntelligenceCommand"/> execution.</summary>
public sealed record CreateAiSafetyIntelligenceResult(
    Guid AiSafetyIntelligenceId,
    string Status);
