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
namespace PlantSafety.Infrastructure.Persistence.Dapper.Queries;

/// <summary>
/// Executes read (SELECT) stored procedures via Dapper on the read-replica connection.
/// Returns mapped DTOs; never returns domain objects or IQueryable.
/// </summary>
public sealed class DapperQueryExecutor(ISqlConnectionFactory connectionFactory, ILogger<DapperQueryExecutor> logger)
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
        return await conn.QuerySingleOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }
}
