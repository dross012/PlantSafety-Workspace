// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Schedules
//  Layer:          Infrastructure
//  File:           SchedulesRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for Schedules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Schedules.Mapping;

/// <summary>Maps Dapper result rows to Schedules read models.</summary>
public static class SchedulesRowMapper
{
    /// <summary>
    /// Maps a raw dictionary row (from a multi-result Dapper query) to a <see cref="SchedulesReadModel"/>.
    /// </summary>
    public static SchedulesReadModel Map(dynamic row)
    {
        return new SchedulesReadModel
        {
            SchedulesId     = (Guid)row.SchedulesId,
            Status       = (string)row.Status,
            TenantId     = (Guid)row.TenantId,
            CreatedAtUtc = (DateTime)row.CreatedAtUtc,
            CreatedBy    = (Guid)row.CreatedBy,
        };
    }
}
