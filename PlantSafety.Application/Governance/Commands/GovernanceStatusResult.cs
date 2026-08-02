// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           GovernanceStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Governance lifecycle-transition commands
//                  (Activate, Complete, Close, Archive). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.AuditingStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Governance.Commands;

/// <summary>Result payload shared by Governance lifecycle-transition commands.</summary>
public sealed record GovernanceStatusResult(Guid GovernanceId, string Status);
