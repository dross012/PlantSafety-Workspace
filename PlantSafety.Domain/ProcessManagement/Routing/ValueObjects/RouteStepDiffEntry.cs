// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteStepDiffEntry.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Single row of a structural diff between two RouteVersionSnapshots, produced by
//                  RouteDiffEngine.Diff. Genuinely new domain concept -- no analog anywhere else
//                  in the platform. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

/// <summary>Single row of a structural diff between two <c>RouteVersionSnapshot</c>s.</summary>
/// <param name="DiffGroup">Logical grouping of the change (currently always "Steps").</param>
/// <param name="FieldName">Human-readable description of what changed.</param>
/// <param name="ChangeType">One of "Addition", "Deletion", "Modification", "Reorder".</param>
/// <param name="PreviousValue">The previous version's value, or <see langword="null"/> for additions.</param>
/// <param name="CurrentValue">The current version's value, or <see langword="null"/> for deletions.</param>
public sealed record RouteStepDiffEntry(
    string DiffGroup,
    string FieldName,
    string ChangeType,
    string? PreviousValue,
    string? CurrentValue);
