// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           DapperQueryExecutor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Executes read stored procedures via Dapper on the read replica.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Options;
using PlantSafety.Infrastructure.Common.Exceptions;

namespace PlantSafety.Infrastructure.Persistence.Dapper.Queries;

/// <summary>
/// Executes read (SELECT) stored procedures via Dapper on the read-replica connection.
/// Returns mapped DTOs; never returns domain objects or IQueryable.
/// </summary>
/// <remarks>
/// FLAGGED (data-access-modernization pass, Checklist Section 2, item 11.2, 2026-07-27):
/// SqlException failures here are always wrapped as a generic
/// <see cref="PersistenceException"/>, unlike PlantSafety.Infrastructure.Common.Exceptions.
/// ErrorClassifier's more granular Conflict/NotFound/ConcurrencyConflict classification used
/// by the six Operations.* admin repositories. Not changed in this pass: routing every
/// mainstream repository's SqlException through ErrorClassifier would change the exception
/// type surfaced platform-wide and needs its own reviewed change, not a drive-by fix. See
/// ErrorClassifier's DRIFT NOTE for the full reasoning.
/// </remarks>
public sealed class DapperQueryExecutor(
    ISqlConnectionFactory connectionFactory,
    ILogger<DapperQueryExecutor> logger,
    IOptions<TenantContextPropagationOptions> tenantContextOptions)
{
    /// <summary>Returns a list of <typeparamref name="T"/> from a stored procedure.</summary>
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string procedureName,
        DynamicParameters parameters,
        CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateReadConnection();

        try
        {
            var tenantId = RequireTenantIdIfEnforced(procedureName, parameters);

            // RLS defense-in-depth: populate SESSION_CONTEXT('TenantId') before touching
            // any Row-Level-Security-protected table. No-ops if the call carries no TenantId
            // (permitted only while enforcement, above, is OFF).
            await conn.SetTenantSessionContextAsync(tenantId, cancellationToken: cancellationToken);

            logger.LogDebug("Query stored procedure {ProcedureName}", procedureName);
            var result = await conn.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Query stored procedure {ProcedureName} failed with SQL error {ErrorNumber}", procedureName, ex.Number);
            throw new PersistenceException(procedureName, string.Empty, ex);
        }
    }

    /// <summary>Returns a single <typeparamref name="T"/> or <see langword="null"/> from a stored procedure.</summary>
    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string procedureName,
        DynamicParameters parameters,
        CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateReadConnection();

        var tenantId = RequireTenantIdIfEnforced(procedureName, parameters);

        // RLS defense-in-depth: populate SESSION_CONTEXT('TenantId') before touching
        // any Row-Level-Security-protected table. No-ops if the call carries no TenantId
        // (permitted only while enforcement, above, is OFF).
        await conn.SetTenantSessionContextAsync(tenantId, cancellationToken: cancellationToken);

        return await conn.QuerySingleOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Returns the call's TenantId (or <see cref="Guid.Empty"/> if absent). When
    /// <see cref="TenantContextPropagationOptions.EnableTenantContextPropagation"/> is ON, absence
    /// of a TenantId parameter is treated as an unmigrated call site and throws instead of
    /// silently no-opping -- see <see cref="TenantContextPropagationOptions"/> remarks.
    /// </summary>
    private Guid RequireTenantIdIfEnforced(string procedureName, DynamicParameters parameters)
    {
        var tenantId = parameters.GetTenantId();
        if (tenantContextOptions.Value.EnableTenantContextPropagation && tenantId == Guid.Empty)
        {
            throw new TenantContextRequiredException(procedureName);
        }

        return tenantId;
    }
}
