// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           TenantSessionContextExtensions.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Sets SQL Server SESSION_CONTEXT('TenantId') on a Dapper connection so
//                  Row-Level Security predicates (Security.fn_TenantAccessPredicate) on
//                  CoreDB/DiagnosticsDB tenant-scoped tables can evaluate correctly.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Dapper;

/// <summary>
/// Ensures SQL Server <c>SESSION_CONTEXT('TenantId')</c> is populated on a connection
/// before any Row-Level-Security-protected table (CoreDB/DiagnosticsDB, ~72 tables secured
/// by <c>Security.fn_TenantAccessPredicate(@TenantId)</c>) is touched.
/// </summary>
/// <remarks>
/// <para>
/// TenantId does not flow through a shared ambient context in the Infrastructure layer
/// (there is no <c>ITenantContextAccessor</c>/<c>ICurrentTenantProvider</c> abstraction
/// below the API layer). Instead every repository method already receives
/// <c>Guid tenantId</c> as an explicit parameter and threads it into the stored-procedure
/// call via <see cref="Persistence.Dapper.SqlBuilder.StoredProcedureBuilder"/>
/// (either as the canonical <c>@TenantId</c> header or an explicit <c>AddGuid("TenantId", ...)</c>
/// call). Both forms normalise to the Dapper <c>DynamicParameters</c> key <c>"TenantId"</c>
/// (Dapper strips the leading <c>@</c> when storing parameters), so this helper reads the
/// tenant id back out of the already-built parameters rather than requiring a new
/// ambient-context abstraction.
/// </para>
/// <para>
/// The underlying statement is idempotent per connection/tenant pair: it only invokes
/// <c>sp_set_session_context</c> when the key is unset or set to a different tenant. This
/// allows the same physical connection to be used for multiple stored-procedure calls
/// within one <c>IUnitOfWork</c> (all for the same tenant) without violating the
/// <c>@read_only = 1</c> guard on the key (a second call to <c>sp_set_session_context</c>
/// against a key already marked read-only would otherwise raise error 15311).
/// </para>
/// <para>
/// <c>@read_only = 1</c> is used deliberately: <see cref="ConnectionFactory.SqlConnectionFactory"/>
/// opens a brand-new <see cref="SqlConnection"/> per repository call (or per
/// <c>IUnitOfWork</c> scope) and disposes it immediately after use via <c>using</c>. ADO.NET
/// issues <c>sp_reset_connection</c> whenever a pooled physical connection is handed back
/// out to a new <see cref="SqlConnection"/> instance, which clears SESSION_CONTEXT — so a
/// pooled connection can never carry a stale tenant id into a different tenant's request.
/// There is no code path in this codebase where a single open connection is reused across
/// two different tenants, so read-only defense-in-depth is safe and does not require
/// mid-lifetime tenant switching.
/// </para>
/// </remarks>
public static class TenantSessionContextExtensions
{
    /// <summary>Dapper <see cref="DynamicParameters"/> key that all repositories use for the tenant id (Dapper strips the leading '@').</summary>
    public const string TenantIdParameterKey = "TenantId";

    private const string SetSessionContextSql = """
        IF TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier) IS NULL
           OR TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier) <> @TenantId
        BEGIN
            EXEC sp_set_session_context @key = N'TenantId', @value = @TenantId, @read_only = 1;
        END
        """;

    /// <summary>
    /// Sets <c>SESSION_CONTEXT('TenantId')</c> to <paramref name="tenantId"/> on
    /// <paramref name="connection"/> (optionally within <paramref name="transaction"/>) so
    /// that Row-Level-Security predicates on CoreDB/DiagnosticsDB tables evaluate correctly
    /// for the remainder of the connection's lifetime. No-ops for <see cref="Guid.Empty"/>
    /// (unresolved tenant context, e.g. anonymous/system paths) and for connections against
    /// databases with no RLS applied (the statement is harmless there).
    /// </summary>
    public static async Task SetTenantSessionContextAsync(
        this IDbConnection connection,
        Guid tenantId,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition(
            SetSessionContextSql,
            new { TenantId = tenantId },
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Extracts the tenant id from a set of stored-procedure <see cref="DynamicParameters"/>
    /// built via <see cref="Persistence.Dapper.SqlBuilder.StoredProcedureBuilder"/>, if present.
    /// Returns <see cref="Guid.Empty"/> when the call does not carry a tenant id
    /// (e.g. system/cross-tenant procedures).
    /// </summary>
    public static Guid GetTenantId(this DynamicParameters parameters)
    {
        return parameters.ParameterNames.Contains(TenantIdParameterKey)
            ? parameters.Get<Guid>(TenantIdParameterKey)
            : Guid.Empty;
    }
}
