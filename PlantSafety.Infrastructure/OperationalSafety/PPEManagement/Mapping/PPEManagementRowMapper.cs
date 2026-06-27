// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.PPEManagement
//  Layer:          Infrastructure
//  File:           PPEManagementRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for PPEManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Mapping;

/// <summary>Maps Dapper result rows to PPEManagement read models.</summary>
public static class PPEManagementRowMapper
{
    /// <summary>
    /// Maps a raw dictionary row (from a multi-result Dapper query) to a <see cref="PPEManagementReadModel"/>.
    /// </summary>
    public static PPEManagementReadModel Map(dynamic row)
    {
        return new PPEManagementReadModel
        {
            PPEManagementId     = (Guid)row.PPEManagementId,
            Status       = (string)row.Status,
            TenantId     = (Guid)row.TenantId,
            CreatedAtUtc = (DateTime)row.CreatedAtUtc,
            CreatedBy    = (Guid)row.CreatedBy,
        };
    }
}
