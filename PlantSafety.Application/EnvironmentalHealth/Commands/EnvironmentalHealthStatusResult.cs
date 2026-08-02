// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           EnvironmentalHealthStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the EnvironmentalHealth lifecycle-transition
//                  commands (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnvironmentalHealth.Commands;

/// <summary>Result payload shared by EnvironmentalHealth lifecycle-transition commands.</summary>
public sealed record EnvironmentalHealthStatusResult(Guid EnvironmentalHealthId, string Status);
