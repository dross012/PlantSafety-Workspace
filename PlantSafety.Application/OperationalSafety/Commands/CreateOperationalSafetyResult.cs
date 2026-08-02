// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CreateOperationalSafetyResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateOperationalSafetyCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Result of a successful <see cref="CreateOperationalSafetyCommand"/> execution.</summary>
public sealed record CreateOperationalSafetyResult(
    Guid OperationalSafetyId,
    string Status);
