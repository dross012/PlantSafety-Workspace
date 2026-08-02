// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           PolicyStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Policy lifecycle-transition commands (Approve,
//                  RecordAcknowledgment, Supersede, Close, Archive). Mirrors
//                  PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Policy.Commands;

/// <summary>Result payload shared by Policy lifecycle-transition commands.</summary>
/// <param name="PolicyId">Identity of the affected Policy aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
/// <param name="AcknowledgmentCount">Current acknowledgment count (unchanged by non-acknowledgment transitions).</param>
public sealed record PolicyStatusResult(Guid PolicyId, string Status, int AcknowledgmentCount);
