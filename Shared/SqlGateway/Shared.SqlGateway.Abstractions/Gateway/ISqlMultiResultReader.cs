// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Abstractions.Gateway
//  File:           ISqlMultiResultReader.cs
//  Created:        2026-08-09
//  Description:    A typed seam over a multi-result-set stored procedure
//                  call (e.g. a single-row count, followed by a page of
//                  rows, followed by a related child row set). Callers
//                  never see the underlying ADO.NET/Dapper reader type.
// ============================================================================
namespace Shared.SqlGateway.Abstractions.Gateway;

/// <summary>A typed seam over a multi-result-set stored procedure call.</summary>
public interface ISqlMultiResultReader : IAsyncDisposable
{
    /// <summary>Reads the next result set as a single typed row (throws if the set is empty).</summary>
    Task<TResult> ReadFirstAsync<TResult>();

    /// <summary>Reads the next result set as a typed row list.</summary>
    Task<IReadOnlyList<TResult>> ReadAsync<TResult>();
}
