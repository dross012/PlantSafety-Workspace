// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           CreateAuditingResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Auditing.Commands;

/// <summary>Result of a successful <see cref="CreateAuditingCommand"/> execution.</summary>
public sealed record CreateAuditingResult(
    Guid AuditingId,
    string Status);
