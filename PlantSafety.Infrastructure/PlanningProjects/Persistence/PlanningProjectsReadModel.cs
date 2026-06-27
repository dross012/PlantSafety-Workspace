// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Infrastructure
//  File:           PlanningProjectsReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model for PlanningProjects queries.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.PlanningProjects.Persistence;

/// <summary>Dapper read model returned by PlanningProjects stored procedures.</summary>
public sealed record PlanningProjectsReadModel
{
    public Guid ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}
