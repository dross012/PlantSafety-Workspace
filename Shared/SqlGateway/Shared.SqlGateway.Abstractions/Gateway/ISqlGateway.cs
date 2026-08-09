// ============================================================================
//  Project:        Shared.SqlGateway
//  Module:         Abstractions.Gateway
//  File:           ISqlGateway.cs
//  Created:        2026-08-09
//  Description:    Phase 2.6 deliverable: the SQL gateway layer. The one
//                  approved seam for calling a SQL Server stored
//                  procedure -- every domain repository (Shared.SafetyData
//                  today; any Phase 3 domain repository built on top of
//                  this) depends on ISqlGateway instead of constructing
//                  its own SqlConnection/Dapper calls. Deterministic by
//                  construction: TParameters and TResult are always
//                  concrete, compile-time types (never `dynamic` or an
//                  untyped DataTable); the procedure name is validated by
//                  SqlIdentifierValidator, so raw SQL text can never flow
//                  through this seam.
// ============================================================================
namespace Shared.SqlGateway.Abstractions.Gateway;

/// <summary>The one approved seam for calling a SQL Server stored procedure.</summary>
public interface ISqlGateway
{
    /// <summary>Calls a stored procedure that returns a single typed result set.</summary>
    Task<IReadOnlyList<TResult>> QueryAsync<TResult>(string storedProcedureName, object parameters, CancellationToken cancellationToken = default);

    /// <summary>Calls a stored procedure that returns multiple result sets, read in order via the returned reader.</summary>
    Task<ISqlMultiResultReader> QueryMultipleAsync(string storedProcedureName, object parameters, CancellationToken cancellationToken = default);
}
