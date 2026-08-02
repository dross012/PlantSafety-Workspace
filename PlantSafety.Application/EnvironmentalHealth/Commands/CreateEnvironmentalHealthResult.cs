// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           CreateEnvironmentalHealthResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateEnvironmentalHealthCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnvironmentalHealth.Commands;

/// <summary>Result of a successful <see cref="CreateEnvironmentalHealthCommand"/> execution.</summary>
public sealed record CreateEnvironmentalHealthResult(
    Guid EnvironmentalHealthId,
    string Status);
