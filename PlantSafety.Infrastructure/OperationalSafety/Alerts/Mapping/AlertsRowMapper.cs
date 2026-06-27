// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Alerts
//  Layer:          Infrastructure
//  File:           AlertsRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for Alerts.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Alerts.Mapping;

/// <summary>Maps Dapper result rows to Alerts read models.</summary>
public static class AlertsRowMapper
{
    /// <summary>
    /// Maps a raw dictionary row (from a multi-result Dapper query) to a <see cref="AlertsReadModel"/>.
    /// </summary>
    public static AlertsReadModel Map(dynamic row)
    {
        return new AlertsReadModel
        {
            AlertsId     = (Guid)row.AlertsId,
            Status       = (string)row.Status,
            TenantId     = (Guid)row.TenantId,
            CreatedAtUtc = (DateTime)row.CreatedAtUtc,
            CreatedBy    = (Guid)row.CreatedBy,
        };
    }
}
