// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteDiffRowDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model row backing the Routing Diff Viewer. Server-computed and
//                  pre-aligned by RouteDiffEngine.Diff -- no client-side diff computation. Shape
//                  matches PlantSafety.UI.Modules.Routing.Models.RoutingDiffRowDto field-for-field.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Single pre-aligned diff row.</summary>
public sealed record RouteDiffRowDto
{
    /// <summary>Diff group (currently always "Steps").</summary>
    public required string DiffGroup { get; init; }

    /// <summary>The field name that changed.</summary>
    public required string FieldName { get; init; }

    /// <summary>Change type (Addition/Deletion/Modification/Reorder).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Previous version's value.</summary>
    public string? PreviousValue { get; init; }

    /// <summary>Current version's value.</summary>
    public string? CurrentValue { get; init; }
}
