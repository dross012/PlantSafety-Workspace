// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure.Persistence
//  Layer:          IntegrationTests
//  File:           TenantSessionContextRlsTests.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Proves that PlantSafety.Infrastructure's Dapper connection/executor
//                  pipeline populates SQL Server SESSION_CONTEXT('TenantId') so that
//                  Row-Level-Security predicates (as applied in Phase 9 to CoreDB/
//                  DiagnosticsDB tenant-scoped tables) resolve correctly, and that
//                  the pre-fix behaviour (no SESSION_CONTEXT set) is denied by RLS.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
using PlantSafety.Infrastructure.Persistence.Dapper;
using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
using PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;

namespace PlantSafety.IntegrationTests.Infrastructure.Persistence;

/// <summary>
/// Verifies the fix for the Phase 9 RLS/SESSION_CONTEXT gap: every connection that
/// touches an RLS-protected table must have <c>SESSION_CONTEXT('TenantId')</c> populated
/// (via <see cref="TenantSessionContextExtensions"/>, wired into
/// <see cref="DapperQueryExecutor"/> and <see cref="DapperCommandExecutor"/>) or all
/// SELECTs return zero rows and all INSERT/UPDATEs are silently blocked by the deny-by-
/// default security policy.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class TenantSessionContextRlsTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static readonly Guid TenantA = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = new("22222222-2222-2222-2222-222222222222");

    /// <summary>
    /// Stands up a minimal analog of the real Phase 9 RLS setup (security predicate
    /// function + deny-by-default FILTER/BLOCK security policy on a tenant-scoped table)
    /// against the shared Testcontainers instance, idempotently.
    /// </summary>
    private async Task EnsureRlsFixtureAsync()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
                EXEC('CREATE SCHEMA [Security]');

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'RlsTest' AND TABLE_NAME = 'Widgets')
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'RlsTest')
                    EXEC('CREATE SCHEMA [RlsTest]');

                EXEC('CREATE TABLE [RlsTest].[Widgets] (
                    WidgetId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId  UNIQUEIDENTIFIER NOT NULL,
                    Name      NVARCHAR(200)    NOT NULL
                );');
            END;
            """);

        // Mirrors the real Security.fn_TenantAccessPredicate(@TenantId) signature/behaviour.
        await conn.ExecuteAsync("""
            IF OBJECT_ID('Security.fn_TenantAccessPredicate_RlsTest', 'IF') IS NOT NULL
                DROP FUNCTION [Security].[fn_TenantAccessPredicate_RlsTest];
            """);

        await conn.ExecuteAsync("""
            EXEC('CREATE FUNCTION [Security].[fn_TenantAccessPredicate_RlsTest](@TenantId UNIQUEIDENTIFIER)
            RETURNS TABLE
            WITH SCHEMABINDING
            AS
            RETURN SELECT 1 AS fn_result
            WHERE @TenantId = CAST(SESSION_CONTEXT(N''TenantId'') AS UNIQUEIDENTIFIER);');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'RlsTestWidgetsPolicy')
            BEGIN
                EXEC('CREATE SECURITY POLICY [Security].[RlsTestWidgetsPolicy]
                    ADD FILTER PREDICATE [Security].[fn_TenantAccessPredicate_RlsTest](TenantId) ON [RlsTest].[Widgets],
                    ADD BLOCK PREDICATE [Security].[fn_TenantAccessPredicate_RlsTest](TenantId) ON [RlsTest].[Widgets] AFTER INSERT
                    WITH (STATE = ON);');
            END;
            """);

        // Canonical-shaped stored procedure so DapperCommandExecutor (which always issues
        // CommandType.StoredProcedure) can be exercised exactly as production repositories use it.
        await conn.ExecuteAsync("""
            IF OBJECT_ID('RlsTest.usp_InsertWidget', 'P') IS NOT NULL
                DROP PROCEDURE [RlsTest].[usp_InsertWidget];
            """);

        await conn.ExecuteAsync("""
            EXEC('CREATE PROCEDURE [RlsTest].[usp_InsertWidget]
                @TenantId UNIQUEIDENTIFIER,
                @WidgetId UNIQUEIDENTIFIER,
                @Name NVARCHAR(200)
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT INTO [RlsTest].[Widgets] (WidgetId, TenantId, Name) VALUES (@WidgetId, @TenantId, @Name);
            END');
            """);

        await conn.ExecuteAsync("""
            IF OBJECT_ID('RlsTest.usp_GetWidgets', 'P') IS NOT NULL
                DROP PROCEDURE [RlsTest].[usp_GetWidgets];
            """);

        await conn.ExecuteAsync("""
            EXEC('CREATE PROCEDURE [RlsTest].[usp_GetWidgets]
                @TenantId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT WidgetId, TenantId, Name FROM [RlsTest].[Widgets];
            END');
            """);

        // Seed one row per tenant (idempotent). The BLOCK PREDICATE requires SESSION_CONTEXT
        // to already match the tenant being inserted, so seed each tenant under its own context.
        await conn.SetTenantSessionContextAsync(TenantA);
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM [RlsTest].[Widgets] WHERE TenantId = @TenantA)
                INSERT INTO [RlsTest].[Widgets] (WidgetId, TenantId, Name) VALUES (NEWID(), @TenantA, 'Tenant A Widget');
            """, new { TenantA });

        await using (var connB = new SqlConnection(ConnectionString))
        {
            await connB.OpenAsync();
            await connB.SetTenantSessionContextAsync(TenantB);
            await connB.ExecuteAsync("""
                IF NOT EXISTS (SELECT 1 FROM [RlsTest].[Widgets] WHERE TenantId = @TenantB)
                    INSERT INTO [RlsTest].[Widgets] (WidgetId, TenantId, Name) VALUES (NEWID(), @TenantB, 'Tenant B Widget');
                """, new { TenantB });
        }
    }

    [Fact]
    public async Task WithoutSessionContext_RlsDeniesAllRows_ProvingTheGapWasReal()
    {
        await EnsureRlsFixtureAsync();

        // Simulates the pre-fix state: a raw connection opened without ever calling
        // sp_set_session_context, exactly as SqlConnectionFactory.CreateConnection()
        // behaved before this change.
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [RlsTest].[Widgets] WHERE TenantId = @TenantA",
            new { TenantA });

        count.Should().Be(0, "deny-by-default RLS blocks every row when SESSION_CONTEXT('TenantId') is never set");
    }

    [Fact]
    public async Task SetTenantSessionContextAsync_ScopesReadsToTheCallingTenant()
    {
        await EnsureRlsFixtureAsync();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.SetTenantSessionContextAsync(TenantA);

        var visibleTenants = (await conn.QueryAsync<Guid>(
            "SELECT DISTINCT TenantId FROM [RlsTest].[Widgets]")).ToList();

        visibleTenants.Should().ContainSingle().Which.Should().Be(TenantA,
            "RLS should only expose Tenant A's rows once SESSION_CONTEXT('TenantId') is set to Tenant A");
    }

    [Fact]
    public async Task DapperQueryExecutor_TheRealSharedChokepoint_SetsSessionContextAutomatically()
    {
        await EnsureRlsFixtureAsync();

        using var scope = Factory.Services.CreateScope();
        var queryExecutor = scope.ServiceProvider.GetRequiredService<DapperQueryExecutor>();

        // Exercises the actual production code path: repositories build DynamicParameters
        // via StoredProcedureBuilder (always including "TenantId") and call
        // DapperQueryExecutor, never touching SESSION_CONTEXT themselves.
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", TenantA)
            .Build();

        var rows = await queryExecutor.QueryAsync<WidgetRow>(
            "[RlsTest].[usp_GetWidgets]",
            parameters);

        rows.Should().ContainSingle();
        rows[0].TenantId.Should().Be(TenantA);
    }

    [Fact]
    public async Task DapperCommandExecutor_RepeatedCallsSameTenant_DoNotViolateTheReadOnlyGuard()
    {
        await EnsureRlsFixtureAsync();

        using var scope = Factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<DapperCommandExecutor>();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var widgetId1 = Guid.NewGuid();
        var widgetId2 = Guid.NewGuid();

        var insert1 = new StoredProcedureBuilder()
            .AddGuid("TenantId", TenantA)
            .AddGuid("WidgetId", widgetId1)
            .AddString("Name", "First")
            .Build();

        var insert2 = new StoredProcedureBuilder()
            .AddGuid("TenantId", TenantA)
            .AddGuid("WidgetId", widgetId2)
            .AddString("Name", "Second")
            .Build();

        const string insertProc = "[RlsTest].[usp_InsertWidget]";

        // Two calls on the same connection/transaction for the SAME tenant must not throw
        // "session context key already set as read-only" (SQL error 15311).
        var act = async () =>
        {
            await commandExecutor.ExecuteAsync(conn, insertProc, insert1, tx, commandTimeout: 30);
            await commandExecutor.ExecuteAsync(conn, insertProc, insert2, tx, commandTimeout: 30);
        };

        await act.Should().NotThrowAsync();

        tx.Commit();

        await using var verifyConn = new SqlConnection(ConnectionString);
        await verifyConn.OpenAsync();
        await verifyConn.SetTenantSessionContextAsync(TenantA);

        var count = await verifyConn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [RlsTest].[Widgets] WHERE WidgetId IN (@Id1, @Id2)",
            new { Id1 = widgetId1, Id2 = widgetId2 });

        count.Should().Be(2, "both inserts within the same unit of work should have succeeded under RLS for Tenant A");
    }

    /// <summary>
    /// Reproduces the "read-only repository" shape used throughout the production codebase
    /// (e.g. <c>ObservationRepository</c>, <c>CultureRepository</c>, and ~40 similar
    /// GetById/GetAll repositories): the method opens a connection via
    /// <see cref="ISqlConnectionFactory.CreateReadConnection"/> but never issues a query on
    /// it directly — the actual SELECT is delegated to <see cref="DapperQueryExecutor"/>,
    /// which opens and session-scopes its own internal connection. This proves that shape is
    /// safe under RLS (the locally-opened connection is inert) even though a naive read of
    /// the call site looks like a bypass of the chokepoint.
    /// </summary>
    [Fact]
    public async Task ReadOnlyRepositoryShape_UnusedLocalReadConnectionPlusQueryExecutor_StillEnforcesRls()
    {
        await EnsureRlsFixtureAsync();

        using var scope = Factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var queryExecutor = scope.ServiceProvider.GetRequiredService<DapperQueryExecutor>();

        // Mirrors production repositories: a read connection is opened locally (and disposed
        // unused) while the real query runs through the shared queryExecutor chokepoint.
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", TenantB)
            .Build();

        var rows = await queryExecutor.QueryAsync<WidgetRow>(
            "[RlsTest].[usp_GetWidgets]",
            parameters);

        rows.Should().ContainSingle("queryExecutor sets SESSION_CONTEXT on its own connection regardless of the caller's unused local connection");
        rows[0].TenantId.Should().Be(TenantB);
    }

    /// <summary>
    /// Reproduces the write-side shape used throughout the production codebase (e.g.
    /// <c>IncidentDapperRepository</c>, <c>ObservationDapperRepository</c>): the repository
    /// opens its own connection/transaction via <c>ISqlConnectionFactory.CreateConnection()</c>
    /// and hands both to <see cref="DapperCommandExecutor"/>, which sets SESSION_CONTEXT on
    /// that exact connection before executing. Proves cross-tenant writes on that connection
    /// are correctly scoped and cannot leak into another tenant's rows.
    /// </summary>
    [Fact]
    public async Task WriteRepositoryShape_CreateConnectionHandedToCommandExecutor_ScopesInsertToCallingTenant()
    {
        await EnsureRlsFixtureAsync();

        using var scope = Factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<DapperCommandExecutor>();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var widgetId = Guid.NewGuid();
        var insert = new StoredProcedureBuilder()
            .AddGuid("TenantId", TenantB)
            .AddGuid("WidgetId", widgetId)
            .AddString("Name", "Tenant B Repo-Shape Widget")
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[RlsTest].[usp_InsertWidget]", insert, tx, commandTimeout: 30);
        tx.Commit();

        await using var verifyConn = new SqlConnection(ConnectionString);
        await verifyConn.OpenAsync();

        // Under Tenant A's context, Tenant B's newly-inserted widget must be invisible.
        await verifyConn.SetTenantSessionContextAsync(TenantA);
        var visibleToTenantA = await verifyConn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [RlsTest].[Widgets] WHERE WidgetId = @widgetId",
            new { widgetId });
        visibleToTenantA.Should().Be(0, "RLS must hide Tenant B's row from a Tenant A session context");

        await using var verifyConnB = new SqlConnection(ConnectionString);
        await verifyConnB.OpenAsync();
        await verifyConnB.SetTenantSessionContextAsync(TenantB);
        var visibleToTenantB = await verifyConnB.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [RlsTest].[Widgets] WHERE WidgetId = @widgetId",
            new { widgetId });
        visibleToTenantB.Should().Be(1, "Tenant B's own session context must see the row it inserted");
    }

    private sealed record WidgetRow(Guid WidgetId, Guid TenantId, string Name);
}
