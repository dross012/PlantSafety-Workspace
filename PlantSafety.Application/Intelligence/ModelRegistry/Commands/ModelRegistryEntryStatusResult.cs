// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           ModelRegistryEntryStatusResult.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the ModelRegistryEntry lifecycle commands (Register,
//                  SubmitForApproval, Approve, Deprecate, Retire). Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Commands.DataLakeActivityStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>Result payload shared by ModelRegistryEntry lifecycle commands.</summary>
/// <param name="ModelRegistryEntryId">Identity of the affected ModelRegistryEntry aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record ModelRegistryEntryStatusResult(Guid ModelRegistryEntryId, string Status);
