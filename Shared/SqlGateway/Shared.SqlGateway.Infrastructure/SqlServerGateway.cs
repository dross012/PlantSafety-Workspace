// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure
//  File:           SqlServerGateway.cs
//  Created:        2026-08-09
//  Description:    Phase 2.6: the real ISqlGateway implementation.
//                  Deterministic by construction (Section 6): every call
//                  targets a stored procedure only (CommandType.StoredProcedure,
//                  never CommandType.Text), the procedure name is checked
//                  by SqlIdentifierValidator before anything touches the
//                  database, and parameters/results are always the
//                  caller's concrete compile-time types -- Dapper maps a
//                  plain object's public properties to named @Parameter
//                  values automatically, so callers never build a
//                  DynamicParameters bag or interpolate values into SQL
//                  text. Every call's diagnostics (Phase 2.6 deliverable)
//                  are logged through Shared.Telemetry's ILogEventWriter
//                  (Phase 2.4), tagged with the current request's
//                  correlation id (Phase 2.4's ICorrelationContext).
// ============================================================================
using System.Diagnostics;

using Dapper;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Policy;
using Shared.SqlGateway.Abstractions.Diagnostics;
using Shared.SqlGateway.Abstractions.Gateway;
using Shared.SqlGateway.Infrastructure.Options;
using Shared.SqlGateway.Infrastructure.Resilience;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SqlGateway.Infrastructure;

/// <summary>The real, Dapper-backed ISqlGateway implementation.</summary>
public sealed class SqlServerGateway(
    IOptions<SqlGatewayOptions> options,
    IClock clock,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext,
    SqlCircuitBreaker sqlCircuitBreaker) : ISqlGateway
{
    private readonly SqlGatewayOptions _options = options.Value;
    private readonly ErrorRetryPolicy _retryPolicy = ErrorRetryPolicy.Standard;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> QueryAsync<TResult>(string storedProcedureName, object parameters, CancellationToken cancellationToken = default)
    {
        SqlIdentifierValidator.EnsureValid(storedProcedureName);
        ArgumentNullException.ThrowIfNull(parameters);

        for (int attempt = 1; ; attempt++)
        {
            if (sqlCircuitBreaker.Breaker.State == CircuitState.Open)
            {
                throw new InvalidOperationException($"SQL Server circuit breaker is open; failing fast without attempting \"{storedProcedureName}\".");
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await using var connection = new SqlConnection(_options.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                IEnumerable<TResult> rows = await connection.QueryAsync<TResult>(
                    new CommandDefinition(
                        storedProcedureName,
                        parameters,
                        commandType: System.Data.CommandType.StoredProcedure,
                        commandTimeout: _options.CommandTimeoutSeconds,
                        cancellationToken: cancellationToken));

                IReadOnlyList<TResult> results = rows.ToList();
                LogDiagnostics(storedProcedureName, parameters, stopwatch.Elapsed, results.Count, succeeded: true, sqlErrorCode: null, errorMessage: null);
                sqlCircuitBreaker.Breaker.RecordSuccess();
                return results;
            }
            catch (SqlException ex)
            {
                LogDiagnostics(storedProcedureName, parameters, stopwatch.Elapsed, rowCount: null, succeeded: false, sqlErrorCode: ex.Number, errorMessage: ex.Message);
                sqlCircuitBreaker.Breaker.RecordFailure();
                if (attempt >= _retryPolicy.MaxAttempts)
                {
                    throw;
                }

                await Task.Delay(_retryPolicy.DelayForAttempt(attempt), cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task<ISqlMultiResultReader> QueryMultipleAsync(string storedProcedureName, object parameters, CancellationToken cancellationToken = default)
    {
        SqlIdentifierValidator.EnsureValid(storedProcedureName);
        ArgumentNullException.ThrowIfNull(parameters);

        // No retry loop here (unlike QueryAsync): the returned ISqlMultiResultReader owns an
        // open connection the caller reads lazily, set by set, after this method returns --
        // retrying inside this method would mean discarding a connection the caller never
        // got a chance to use. Circuit breaker fail-fast and failure recording still apply.
        if (sqlCircuitBreaker.Breaker.State == CircuitState.Open)
        {
            throw new InvalidOperationException($"SQL Server circuit breaker is open; failing fast without attempting \"{storedProcedureName}\".");
        }

        var stopwatch = Stopwatch.StartNew();
        var connection = new SqlConnection(_options.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);

            SqlMapper.GridReader gridReader = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    storedProcedureName,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            // Row count isn't known upfront for a multi-result-set call (results are read
            // lazily, set by set, by the caller) -- diagnostics for this path report the
            // call as succeeded once the procedure itself has started returning data.
            LogDiagnostics(storedProcedureName, parameters, stopwatch.Elapsed, rowCount: null, succeeded: true, sqlErrorCode: null, errorMessage: null);
            sqlCircuitBreaker.Breaker.RecordSuccess();
            return new DapperSqlMultiResultReader(connection, gridReader);
        }
        catch (SqlException ex)
        {
            await connection.DisposeAsync();
            LogDiagnostics(storedProcedureName, parameters, stopwatch.Elapsed, rowCount: null, succeeded: false, sqlErrorCode: ex.Number, errorMessage: ex.Message);
            sqlCircuitBreaker.Breaker.RecordFailure();
            throw;
        }
    }

    private void LogDiagnostics(string storedProcedureName, object parameters, TimeSpan elapsed, int? rowCount, bool succeeded, int? sqlErrorCode, string? errorMessage)
    {
        IReadOnlyList<string> parameterNames = parameters.GetType().GetProperties().Select(p => p.Name).ToArray();
        var diagnostics = new SqlCallDiagnostics(storedProcedureName, parameterNames, elapsed, rowCount, succeeded, sqlErrorCode, errorMessage);
        logEventWriter.Write(SqlDiagnosticsLogEventFactory.ToLogEvent(diagnostics, correlationContext.CorrelationId, clock.UtcNow));
    }
}
