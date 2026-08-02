// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           DigitalTwinStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the DigitalTwin lifecycle-transition commands
//                  (Activate, RefreshSync, Complete, Close, Archive). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Result payload shared by DigitalTwin lifecycle-transition commands.</summary>
public sealed record DigitalTwinStatusResult(Guid DigitalTwinId, string Status);
