// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           AiSafetyIntelligenceStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the AiSafetyIntelligence lifecycle-transition
//                  commands (Activate, Complete, Close, Archive). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiSafetyIntelligence.Commands;

/// <summary>Result payload shared by AiSafetyIntelligence lifecycle-transition commands.</summary>
public sealed record AiSafetyIntelligenceStatusResult(Guid AiSafetyIntelligenceId, string Status);
