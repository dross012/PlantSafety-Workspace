// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           DataLakeActivityStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the DataLakeActivity lifecycle commands (Create,
//                  StartProcessing, Curate, Fail, Archive). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Commands.RCAStatusResult's
//                  precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.DataLake.Commands;

/// <summary>Result payload shared by DataLakeActivity lifecycle commands.</summary>
/// <param name="DataLakeActivityId">Identity of the affected DataLakeActivity aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record DataLakeActivityStatusResult(Guid DataLakeActivityId, string Status);
