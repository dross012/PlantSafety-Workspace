// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CreateHazmatContainerResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateHazmatContainerCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Result of a successful <see cref="CreateHazmatContainerCommand"/> execution.</summary>
/// <param name="HazmatContainerId">Identity of the newly created HazmatContainer aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateHazmatContainerResult(
    Guid HazmatContainerId,
    string Status);
