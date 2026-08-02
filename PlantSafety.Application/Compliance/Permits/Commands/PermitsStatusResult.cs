// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           PermitsStatusResult.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Permits lifecycle-transition commands
//                  (Approve, Reject, Complete, Expire, Close, Archive). Mirrors
//                  RiskStatusResult / AccidentStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Result payload shared by Permits lifecycle-transition commands.</summary>
/// <param name="PermitsId">Identity of the affected Permits aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PermitsStatusResult(Guid PermitsId, string Status);
