// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           SqlServerDistributedLockProvider.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    SQL-Server-backed IDistributedLockProvider using the built-in
//                  sp_getapplock/sp_releaseapplock system stored procedures. This platform is SQL
//                  Server only (Dapper + stored procedures; no Redis/Postgres advisory locks in
//                  the persistence stack -- StackExchange.Redis is present only for
//                  IDistributedCache, an entirely separate concern), so application locks are the
//                  correct, dependency-free choice for cross-process mutual exclusion.
//
//                  sp_getapplock/sp_releaseapplock are system stored procedures resolvable
//                  unqualified from any database context (the same "sp_" special-resolution
//                  behavior already relied on by TenantSessionContextExtensions'
//                  sp_set_session_context call) -- no custom wrapper .sql procedure is required,
//                  matching this codebase's existing convention of calling system procs directly
//                  via Dapper (see TenantSessionContextExtensions.SetTenantSessionContextAsync).
//
//                  CONNECTION-SCOPING FOOTGUN (deliberately avoided): application locks acquired
//                  with @LockOwner = 'Session' are scoped to the *physical connection*, not to a
//                  transaction. Every acquired lock therefore owns a dedicated SqlConnection
//                  (opened via ISqlConnectionFactory.CreateConnection(), never returned to the
//                  pool while held) for its entire lifetime; the connection is closed only when
//                  the returned lock handle is disposed, which also releases the lock. The lock is
//                  never released early by ADO.NET connection pooling because the connection is
//                  held open explicitly by the caller's `await using` for the whole critical
//                  section, exactly mirroring how WorkflowInstanceDapperRepository holds its
//                  transaction's connection open for the duration of a unit of work.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Common.Exceptions;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Common.Concurrency;

/// <summary>
/// <see cref="IDistributedLockProvider"/> backed by SQL Server application locks
/// (<c>sp_getapplock</c>/<c>sp_releaseapplock</c>). Safe under connection pooling: each acquired
/// lock owns a dedicated, explicitly-held-open <see cref="SqlConnection"/> for its entire
/// lifetime, released only when the returned handle is disposed.
/// </summary>
public sealed class SqlServerDistributedLockProvider(
    ISqlConnectionFactory connectionFactory,
    ILogger<SqlServerDistributedLockProvider> logger) : IDistributedLockProvider
{
    /// <summary>Maximum length SQL Server's sp_getapplock accepts for @Resource (nvarchar(255)).</summary>
    private const int MaxResourceKeyLength = 255;

    /// <summary>Lock mode: always exclusive -- this provider offers only mutual-exclusion semantics.</summary>
    private const string ExclusiveLockMode = "Exclusive";

    /// <summary>Lock owner: scoped to the dedicated connection held open by the returned handle.</summary>
    private const string SessionLockOwner = "Session";

    /// <inheritdoc/>
    public async Task<IAsyncDisposable> AcquireAsync(string resourceKey, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var handle = await TryAcquireAsync(resourceKey, timeout, cancellationToken);
        if (handle is null)
        {
            throw new DistributedLockTimeoutException(NormalizeResourceKey(resourceKey), timeout);
        }

        return handle;
    }

    /// <inheritdoc/>
    public async Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
        {
            throw new ArgumentException("Resource key must not be null or whitespace.", nameof(resourceKey));
        }

        var normalizedKey = NormalizeResourceKey(resourceKey);
        var connection = connectionFactory.CreateConnection();

        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Resource", normalizedKey, DbType.String, size: MaxResourceKeyLength);
            parameters.Add("@LockMode", ExclusiveLockMode, DbType.String, size: 32);
            parameters.Add("@LockOwner", SessionLockOwner, DbType.String, size: 32);
            parameters.Add("@LockTimeout", ClampTimeoutMilliseconds(timeout), DbType.Int32);
            parameters.Add("@DbPrincipal", "public", DbType.String, size: 32);
            parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            await connection.ExecuteAsync(new CommandDefinition(
                "sp_getapplock",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

            var result = parameters.Get<int>("@Result");

            // sp_getapplock return codes: 0/1/2/3 = lock granted (0 = immediately, others after
            // waiting/converting); negative = not granted (-1 timeout, -2 canceled, -3 deadlock
            // victim, -999 parameter/other error). See Microsoft Learn "sp_getapplock (Transact-SQL)".
            if (result < 0)
            {
                logger.LogWarning(
                    "DistributedLockAcquireFailed | ResourceKey={ResourceKey} | ResultCode={ResultCode} | TimeoutMs={TimeoutMs}",
                    normalizedKey, result, ClampTimeoutMilliseconds(timeout));

                connection.Dispose();
                return null;
            }

            logger.LogDebug(
                "DistributedLockAcquired | ResourceKey={ResourceKey} | ResultCode={ResultCode}",
                normalizedKey, result);

            return new SqlServerDistributedLockHandle(connection, normalizedKey, logger);
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    /// <summary>Truncates over-length resource keys to sp_getapplock's nvarchar(255) @Resource limit.</summary>
    private static string NormalizeResourceKey(string resourceKey)
        => resourceKey.Length > MaxResourceKeyLength ? resourceKey[..MaxResourceKeyLength] : resourceKey;

    /// <summary>Clamps a requested timeout to the non-negative Int32 millisecond range sp_getapplock's @LockTimeout accepts.</summary>
    private static int ClampTimeoutMilliseconds(TimeSpan timeout)
    {
        var ms = timeout.TotalMilliseconds;
        if (ms < 0)
        {
            return 0;
        }

        return ms > int.MaxValue ? int.MaxValue : (int)ms;
    }
}
