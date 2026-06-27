// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Checklists
//  Layer:          Infrastructure
//  File:           ChecklistsRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for Checklists.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Checklists.Mapping;

/// <summary>Maps Dapper result rows to Checklists read models.</summary>
public static class ChecklistsRowMapper
{
    /// <summary>
    /// Maps a raw dictionary row (from a multi-result Dapper query) to a <see cref="ChecklistsReadModel"/>.
    /// </summary>
    public static ChecklistsReadModel Map(dynamic row)
    {
        return new ChecklistsReadModel
        {
            ChecklistsId     = (Guid)row.ChecklistsId,
            Status       = (string)row.Status,
            TenantId     = (Guid)row.TenantId,
            CreatedAtUtc = (DateTime)row.CreatedAtUtc,
            CreatedBy    = (Guid)row.CreatedBy,
        };
    }
}
