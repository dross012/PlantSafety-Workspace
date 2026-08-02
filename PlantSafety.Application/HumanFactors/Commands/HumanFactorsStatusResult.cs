// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           HumanFactorsStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the HumanFactors lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.HumanFactors.Commands;

/// <summary>Result payload shared by HumanFactors lifecycle-transition commands.</summary>
public sealed record HumanFactorsStatusResult(Guid HumanFactorsId, string Status);
