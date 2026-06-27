// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training.CompetencyManagement
//  Layer:          Infrastructure
//  File:           CompetencyManagementRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for CompetencyManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Training.CompetencyManagement.Mapping;

/// <summary>Maps Dapper result rows to <see cref="CompetencyManagementReadModel"/>.</summary>
public static class CompetencyManagementRowMapper
{
    /// <summary>Maps a dynamic Dapper row to a read model.</summary>
    public static CompetencyManagementReadModel Map(dynamic row) => new()
    {
        CompetencyManagementId     = (Guid)row.CompetencyManagementId,
        Status       = (string)row.Status,
        TenantId     = (Guid)row.TenantId,
        CreatedAtUtc = (DateTime)row.CreatedAtUtc,
        CreatedBy    = (Guid)row.CreatedBy,
    };
}
