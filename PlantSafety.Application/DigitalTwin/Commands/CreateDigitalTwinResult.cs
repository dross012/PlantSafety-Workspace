// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           CreateDigitalTwinResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateDigitalTwinCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Result of a successful <see cref="CreateDigitalTwinCommand"/> execution.</summary>
public sealed record CreateDigitalTwinResult(
    Guid DigitalTwinId,
    string Status);
