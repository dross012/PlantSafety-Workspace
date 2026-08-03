// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Infrastructure
//  File:           RouteStepJsonModel.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Shared JSON (de)serialization row shape for a single route step, persisted as
//                  a JSON array in the [StepsJson] column of both
//                  [ProcessManagement].[RouteDefinitionAggregate] (the live, editable step list)
//                  and [ProcessManagement].[RouteVersionSnapshot] (an immutable historical
//                  capture) -- both tables share the identical step shape, so one JSON model
//                  serves both repositories. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Infrastructure.ProcessManagement.Routing.Persistence;

/// <summary>Shared JSON row shape for a single persisted route step.</summary>
internal sealed record RouteStepJsonModel
{
    public Guid StepId { get; init; }
    public int StepNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public List<string> Resources { get; init; } = [];
    public int EstimatedDurationMinutes { get; init; }
}

/// <summary>Shared (de)serialization helpers for the <see cref="RouteStepJsonModel"/> array column.</summary>
internal static class RouteStepJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = null };

    /// <summary>Serializes a set of step snapshots to their JSON column representation.</summary>
    public static string Serialize(IEnumerable<RouteStepSnapshot> steps)
    {
        var rows = steps
            .OrderBy(s => s.StepNumber)
            .Select(s => new RouteStepJsonModel
            {
                StepId = s.StepId,
                StepNumber = s.StepNumber,
                Name = s.Name,
                Instructions = s.Instructions,
                Resources = [.. s.Resources],
                EstimatedDurationMinutes = s.EstimatedDurationMinutes,
            })
            .ToList();

        return JsonSerializer.Serialize(rows, Options);
    }

    /// <summary>Deserializes the JSON column representation back into step snapshots, ordered by StepNumber.</summary>
    public static List<RouteStepSnapshot> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        var rows = JsonSerializer.Deserialize<List<RouteStepJsonModel>>(json, Options) ?? [];

        return rows
            .OrderBy(r => r.StepNumber)
            .Select(r => new RouteStepSnapshot(r.StepId, r.StepNumber, r.Name, r.Instructions, r.Resources, r.EstimatedDurationMinutes))
            .ToList();
    }
}
