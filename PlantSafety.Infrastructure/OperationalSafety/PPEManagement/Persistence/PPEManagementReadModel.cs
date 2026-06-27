// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.PPEManagement
//  Layer:          Infrastructure
//  File:           PPEManagementReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for PPEManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Persistence;

/// <summary>Dapper read model returned by PPEManagement stored procedures.</summary>
public sealed record PPEManagementReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid PPEManagementId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
