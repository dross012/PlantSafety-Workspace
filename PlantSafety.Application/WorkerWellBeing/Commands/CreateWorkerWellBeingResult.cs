// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           CreateWorkerWellBeingResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateWorkerWellBeingCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.WorkerWellBeing.Commands;

/// <summary>Result of a successful <see cref="CreateWorkerWellBeingCommand"/> execution.</summary>
public sealed record CreateWorkerWellBeingResult(
    Guid WorkerWellBeingId,
    string Status);
