// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           CreateHumanFactorsResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateHumanFactorsCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.Commands;

/// <summary>Result of a successful <see cref="CreateHumanFactorsCommand"/> execution.</summary>
public sealed record CreateHumanFactorsResult(
    Guid HumanFactorsId,
    string Status);
