// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           ProceduresStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the Procedures lifecycle-transition commands (Approve,
//                  Supersede, Close, Archive). Mirrors PermitsStatusResult's precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Result payload shared by Procedures lifecycle-transition commands.</summary>
/// <param name="ProceduresId">Identity of the affected Procedures aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ProceduresStatusResult(Guid ProceduresId, string Status);
