// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.RiskManagement
//  Layer:          Infrastructure
//  File:           RiskManagementRowMapper.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper row-to-domain mapper for RiskManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Mapping;

/// <summary>Maps Dapper result rows to RiskManagement read models.</summary>
public static class RiskManagementRowMapper
{
    /// <summary>
    /// Maps a raw dictionary row (from a multi-result Dapper query) to a <see cref="RiskManagementReadModel"/>.
    /// </summary>
    public static RiskManagementReadModel Map(dynamic row)
    {
        return new RiskManagementReadModel
        {
            RiskManagementId     = (Guid)row.RiskManagementId,
            Status       = (string)row.Status,
            TenantId     = (Guid)row.TenantId,
            CreatedAtUtc = (DateTime)row.CreatedAtUtc,
            CreatedBy    = (Guid)row.CreatedBy,
        };
    }
}
