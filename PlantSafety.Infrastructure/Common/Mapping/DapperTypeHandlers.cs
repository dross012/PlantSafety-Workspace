// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           DapperTypeHandlers.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper SqlMapper.TypeHandler implementations for all domain value types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Mapping;

/// <summary>
/// Registers Dapper <see cref="SqlMapper.TypeHandler{T}"/> implementations for
/// PlantSafety domain value types (StrongIds, UtcTimestamp).
/// </summary>
public static class DapperTypeHandlers
{
    /// <summary>Call once at startup to register all custom type handlers.</summary>
    public static void RegisterAll()
    {
        SqlMapper.AddTypeHandler(new UtcTimestampHandler());
        SqlMapper.AddTypeHandler(new NullableUtcTimestampHandler());
    }

    /// <summary>Maps DATETIME2 ↔ UtcTimestamp.</summary>
    private sealed class UtcTimestampHandler : SqlMapper.TypeHandler<UtcTimestamp>
    {
        public override void SetValue(IDbDataParameter parameter, UtcTimestamp value)
        {
            parameter.Value  = value.Value;
            parameter.DbType = DbType.DateTime2;
        }

        public override UtcTimestamp Parse(object value)
        {
            var dt = (DateTime)value;
            return new UtcTimestamp(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        }
    }

    /// <summary>Maps nullable DATETIME2 ↔ UtcTimestamp?.</summary>
    private sealed class NullableUtcTimestampHandler : SqlMapper.TypeHandler<UtcTimestamp?>
    {
        public override void SetValue(IDbDataParameter parameter, UtcTimestamp? value)
        {
            parameter.Value  = value.HasValue ? value.Value.Value : DBNull.Value;
            parameter.DbType = DbType.DateTime2;
        }

        public override UtcTimestamp? Parse(object value)
        {
            if (value is DBNull || value is null) return null;
            var dt = (DateTime)value;
            return new UtcTimestamp(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        }
    }
}
