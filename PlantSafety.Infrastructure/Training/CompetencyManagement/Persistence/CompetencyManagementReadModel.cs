// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training.CompetencyManagement
//  Layer:          Infrastructure
//  File:           CompetencyManagementReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for CompetencyManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Training.CompetencyManagement.Persistence;

/// <summary>Dapper read model returned by CompetencyManagement stored procedures.</summary>
public sealed record CompetencyManagementReadModel
{
    public Guid CompetencyManagementId { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}
