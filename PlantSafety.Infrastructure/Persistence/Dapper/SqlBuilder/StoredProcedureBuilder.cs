// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           StoredProcedureBuilder.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Fluent builder for Dapper DynamicParameters targeting canonical stored procedure signatures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;

/// <summary>
/// Fluent builder for Dapper <see cref="DynamicParameters"/> targeting canonical stored
/// procedure signatures that always include @TenantId, @CorrelationId, @ActorId, @NowUtc.
/// </summary>
public sealed class StoredProcedureBuilder
{
    private readonly DynamicParameters _params = new();

    /// <summary>Adds the four canonical header parameters required by every stored procedure.</summary>
    public StoredProcedureBuilder WithCanonicalHeaders(
        Guid tenantId, Guid correlationId, Guid actorId, DateTime nowUtc)
    {
        _params.Add(SqlConstants.ParamTenantId,     tenantId,     DbType.Guid);
        _params.Add(SqlConstants.ParamCorrelationId, correlationId, DbType.Guid);
        _params.Add(SqlConstants.ParamActorId,       actorId,      DbType.Guid);
        _params.Add(SqlConstants.ParamNowUtc,        nowUtc,       DbType.DateTime2);
        return this;
    }

    /// <summary>Adds a UNIQUEIDENTIFIER parameter.</summary>
    public StoredProcedureBuilder AddGuid(string name, Guid value)
    {
        _params.Add(name, value, DbType.Guid);
        return this;
    }

    /// <summary>Adds a nullable UNIQUEIDENTIFIER parameter.</summary>
    public StoredProcedureBuilder AddGuid(string name, Guid? value)
    {
        _params.Add(name, value, DbType.Guid);
        return this;
    }

    /// <summary>Adds an NVARCHAR parameter.</summary>
    public StoredProcedureBuilder AddString(string name, string? value, int size = 4000)
    {
        _params.Add(name, value, DbType.String, size: size);
        return this;
    }

    /// <summary>Adds a DATETIME2 parameter.</summary>
    public StoredProcedureBuilder AddDateTime(string name, DateTime value)
    {
        _params.Add(name, value, DbType.DateTime2);
        return this;
    }

    /// <summary>Adds a nullable DATETIME2 parameter.</summary>
    public StoredProcedureBuilder AddDateTime(string name, DateTime? value)
    {
        _params.Add(name, value, DbType.DateTime2);
        return this;
    }

    /// <summary>Adds a BIT parameter.</summary>
    public StoredProcedureBuilder AddBool(string name, bool value)
    {
        _params.Add(name, value, DbType.Boolean);
        return this;
    }

    /// <summary>Adds an INT parameter.</summary>
    public StoredProcedureBuilder AddInt(string name, int value)
    {
        _params.Add(name, value, DbType.Int32);
        return this;
    }

    /// <summary>Builds the <see cref="DynamicParameters"/> instance.</summary>
    public DynamicParameters Build() => _params;
}
