// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Infrastructure
//  File:           DapperSqlMultiResultReader.cs
//  Created:        2026-08-09
//  Description:    Wraps Dapper's SqlMapper.GridReader behind
//                  ISqlMultiResultReader, and owns the underlying
//                  SqlConnection's lifetime -- disposing the reader closes
//                  the connection, mirroring the "using var connection"
//                  pattern the pre-existing SafetyDataDapperRepository
//                  used directly before this gateway existed.
// ============================================================================
using Dapper;

using Microsoft.Data.SqlClient;

using Shared.SqlGateway.Abstractions.Gateway;

namespace Shared.SqlGateway.Infrastructure;

/// <summary>Wraps a Dapper GridReader behind ISqlMultiResultReader, owning the connection's lifetime.</summary>
internal sealed class DapperSqlMultiResultReader(SqlConnection connection, SqlMapper.GridReader gridReader) : ISqlMultiResultReader
{
    /// <inheritdoc />
    public Task<TResult> ReadFirstAsync<TResult>() => gridReader.ReadFirstAsync<TResult>();

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> ReadAsync<TResult>() => (await gridReader.ReadAsync<TResult>()).ToList();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        gridReader.Dispose();
        await connection.DisposeAsync();
    }
}
