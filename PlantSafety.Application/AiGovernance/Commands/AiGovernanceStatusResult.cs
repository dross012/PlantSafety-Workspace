// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           AiGovernanceStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the AiGovernance lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.AiGovernance.Commands;

/// <summary>Result payload shared by AiGovernance lifecycle-transition commands.</summary>
public sealed record AiGovernanceStatusResult(Guid AiGovernanceId, string Status);
