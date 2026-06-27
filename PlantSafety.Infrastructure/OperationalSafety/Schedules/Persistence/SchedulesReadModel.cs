// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Schedules
//  Layer:          Infrastructure
//  File:           SchedulesReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for Schedules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Schedules.Persistence;

/// <summary>Dapper read model returned by Schedules stored procedures.</summary>
public sealed record SchedulesReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid SchedulesId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
