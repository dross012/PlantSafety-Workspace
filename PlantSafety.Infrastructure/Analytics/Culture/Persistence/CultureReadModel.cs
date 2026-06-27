// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for Culture queries.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper read model returned by Culture query stored procedures.</summary>
public sealed record CultureReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid CultureId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
