// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           OperationalSafetyStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the OperationalSafety lifecycle-transition
//                  commands (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Result payload shared by OperationalSafety lifecycle-transition commands.</summary>
public sealed record OperationalSafetyStatusResult(Guid OperationalSafetyId, string Status);
