// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           AuditingStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Auditing lifecycle-transition commands
//                  (Start, RecordFindings, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Auditing.Commands;

/// <summary>Result payload shared by Auditing lifecycle-transition commands.</summary>
public sealed record AuditingStatusResult(Guid AuditingId, string Status);
