// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           CreateProcessDefinitionResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreateProcessDefinitionCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Result of a successful <see cref="CreateProcessDefinitionCommand"/> execution.</summary>
public sealed record CreateProcessDefinitionResult(
    Guid ProcessDefinitionId,
    string Status);
