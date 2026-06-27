// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.RiskManagement
//  Layer:          Infrastructure
//  File:           RiskManagementReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for RiskManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Persistence;

/// <summary>Dapper read model returned by RiskManagement stored procedures.</summary>
public sealed record RiskManagementReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid RiskManagementId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
