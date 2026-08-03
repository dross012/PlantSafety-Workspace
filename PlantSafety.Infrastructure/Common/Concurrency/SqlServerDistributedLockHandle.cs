// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           SqlServerDistributedLockHandle.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    IAsyncDisposable handle for a SQL Server application lock acquired by
//                  SqlServerDistributedLockProvider. Releases the lock via sp_releaseapplock and
//                  closes the dedicated connection that held it open. Disposal is idempotent and
//                  best-effort: a failure to explicitly release never throws out of DisposeAsync
//                  (the lock is released regardless, at the latest, when the underlying connection
//                  is closed/disposed -- application locks with @LockOwner = 'Session' cannot
//                  outlive their owning connection).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Common.Concurrency;

/// <summary>
/// <see cref="IAsyncDisposable"/> handle representing a held SQL Server application lock.
/// Disposing the handle releases the lock (via <c>sp_releaseapplock</c>) and closes the dedicated
/// connection that held it open. See <see cref="SqlServerDistributedLockProvider"/>.
/// </summary>
internal sealed class SqlServerDistributedLockHandle : IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private readonly string _resourceKey;
    private readonly ILogger _logger;
    private int _disposed;

    /// <summary>Creates a handle for a lock already granted on <paramref name="connection"/>.</summary>
    internal SqlServerDistributedLockHandle(SqlConnection connection, string resourceKey, ILogger logger)
    {
        _connection = connection;
        _resourceKey = resourceKey;
        _logger = logger;
    }

    /// <summary>
    /// Releases the application lock via <c>sp_releaseapplock</c> and disposes the dedicated
    /// connection. Safe to call more than once; only the first call performs work.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        try
        {
            if (_connection.State == ConnectionState.Open)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Resource", _resourceKey, DbType.String, size: 255);
                parameters.Add("@LockOwner", "Session", DbType.String, size: 32);

                await _connection.ExecuteAsync(new CommandDefinition(
                    "sp_releaseapplock",
                    parameters,
                    commandType: CommandType.StoredProcedure));

                _logger.LogDebug("DistributedLockReleased | ResourceKey={ResourceKey}", _resourceKey);
            }
        }
        catch (Exception ex)
        {
            // Best-effort: even if the explicit release fails (e.g. connection already faulted),
            // disposing the connection below guarantees the lock cannot outlive it.
            _logger.LogWarning(ex, "DistributedLockReleaseFailed | ResourceKey={ResourceKey}", _resourceKey);
        }
        finally
        {
            _connection.Dispose();
        }
    }
}
