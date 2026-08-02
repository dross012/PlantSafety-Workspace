// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           CreateProceduresResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateProceduresCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Result of a successful <see cref="CreateProceduresCommand"/> execution.</summary>
/// <param name="ProceduresId">Identity of the newly created Procedures aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateProceduresResult(
    Guid ProceduresId,
    string Status);
