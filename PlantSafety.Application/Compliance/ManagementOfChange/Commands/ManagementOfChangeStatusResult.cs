// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           ManagementOfChangeStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ManagementOfChange lifecycle-transition commands
//                  (Approve, Implement, Close, Archive). Mirrors PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Result payload shared by ManagementOfChange lifecycle-transition commands.</summary>
/// <param name="ManagementOfChangeId">Identity of the affected ManagementOfChange aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ManagementOfChangeStatusResult(Guid ManagementOfChangeId, string Status);
