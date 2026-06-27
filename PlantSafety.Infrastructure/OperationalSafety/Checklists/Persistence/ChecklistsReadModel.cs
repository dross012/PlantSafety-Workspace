// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Checklists
//  Layer:          Infrastructure
//  File:           ChecklistsReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for Checklists.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Checklists.Persistence;

/// <summary>Dapper read model returned by Checklists stored procedures.</summary>
public sealed record ChecklistsReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid ChecklistsId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
