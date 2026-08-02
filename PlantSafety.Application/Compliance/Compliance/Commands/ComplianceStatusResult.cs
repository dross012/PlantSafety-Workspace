// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           ComplianceStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the (self-named) Compliance lifecycle-transition
//                  commands (Activate, Evaluate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Result payload shared by Compliance lifecycle-transition commands.</summary>
public sealed record ComplianceStatusResult(Guid ComplianceId, string Status);
