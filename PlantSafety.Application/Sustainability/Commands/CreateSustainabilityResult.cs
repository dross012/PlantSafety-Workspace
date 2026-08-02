// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CreateSustainabilityResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateSustainabilityCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.Commands;

/// <summary>Result of a successful <see cref="CreateSustainabilityCommand"/> execution.</summary>
public sealed record CreateSustainabilityResult(
    Guid SustainabilityId,
    string Status);
