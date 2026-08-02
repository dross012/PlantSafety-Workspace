// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           WorkerWellBeingStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the WorkerWellBeing lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.WorkerWellBeing.Commands;

/// <summary>Result payload shared by WorkerWellBeing lifecycle-transition commands.</summary>
public sealed record WorkerWellBeingStatusResult(Guid WorkerWellBeingId, string Status);
