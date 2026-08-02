// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           SustainabilityStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Sustainability lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.Commands;

/// <summary>Result payload shared by Sustainability lifecycle-transition commands.</summary>
public sealed record SustainabilityStatusResult(Guid SustainabilityId, string Status);
