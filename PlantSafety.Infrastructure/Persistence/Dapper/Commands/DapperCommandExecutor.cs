// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           DapperCommandExecutor.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Executes write stored procedures via Dapper within a supplied IDbTransaction.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Dapper.Commands;

/// <summary>
/// Executes write (INSERT/UPDATE) stored procedures via Dapper within a supplied
/// <see cref="IDbTransaction"/>. All parameters are explicit; no dynamic SQL.
/// </summary>
public sealed class DapperCommandExecutor(ILogger<DapperCommandExecutor> logger)
{
    /// <summary>
    /// Executes a stored procedure and returns the number of rows affected.
    /// </summary>
    public async Task<int> ExecuteAsync(
        IDbConnection connection,
        string procedureName,
        DynamicParameters parameters,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // RLS defense-in-depth: populate SESSION_CONTEXT('TenantId') before touching
            // any Row-Level-Security-protected table. Idempotent per connection/tenant, so
            // repeated calls within the same IUnitOfWork/transaction for the same tenant are
            // safe. No-ops if the call carries no TenantId.
            await connection.SetTenantSessionContextAsync(parameters.GetTenantId(), transaction, cancellationToken);

            logger.LogDebug("Executing stored procedure {ProcedureName}", procedureName);

            return await connection.ExecuteAsync(
                sql: procedureName,
                param: parameters,
                transaction: transaction,
                commandTimeout: commandTimeout ?? 30,
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Stored procedure {ProcedureName} failed with SQL error {ErrorNumber}", procedureName, ex.Number);
            throw new PersistenceException(procedureName, string.Empty, ex);
        }
    }
}
