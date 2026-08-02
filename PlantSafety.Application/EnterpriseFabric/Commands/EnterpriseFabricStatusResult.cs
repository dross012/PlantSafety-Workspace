// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           EnterpriseFabricStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the EnterpriseFabric lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnterpriseFabric.Commands;

/// <summary>Result payload shared by EnterpriseFabric lifecycle-transition commands.</summary>
public sealed record EnterpriseFabricStatusResult(Guid EnterpriseFabricId, string Status);
