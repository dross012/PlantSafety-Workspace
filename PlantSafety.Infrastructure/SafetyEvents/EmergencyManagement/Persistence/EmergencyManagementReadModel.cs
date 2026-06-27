// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Infrastructure
//  File:           EmergencyManagementReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for EmergencyManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;

/// <summary>Dapper read model returned by EmergencyManagement stored procedures.</summary>
public sealed record EmergencyManagementReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid EmergencyManagementId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }
}
