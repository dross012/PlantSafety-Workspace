// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           CreatePermitsResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Result of a successful <see cref="CreatePermitsCommand"/> execution.</summary>
/// <param name="PermitsId">Identity of the newly created Permits aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreatePermitsResult(
    Guid PermitsId,
    string Status);
