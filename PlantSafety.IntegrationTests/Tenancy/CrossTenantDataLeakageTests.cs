// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Tenancy
//  Layer:          IntegrationTests
//  File:           CrossTenantDataLeakageTests.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.1)
//  SchemaVersion:  2026.1
//  Description:    Real, end-to-end proof that cross-tenant data leakage is prevented for
//                  genuine business aggregates, at two independent layers of defense-in-depth:
//                    1. Application-layer scoping: IAccidentRepository.GetByIdAsync/UpdateAsync
//                       (the real, DI-resolved AccidentDapperRepository) push TenantId into the
//                       stored-procedure WHERE clause itself -- Tenant B's caller can never
//                       retrieve or mutate Tenant A's row, regardless of RLS.
//                    2. Database-layer defense-in-depth: a REAL Row-Level-Security FILTER +
//                       BLOCK security policy (mirroring the production
//                       Security.fn_TenantAccessPredicate mechanism from
//                       Persistence/Migrations/Phase9-Security/90-row-level-security.sql, applied
//                       here directly to the real [AccidentManagement].[Accident] table -- not a
//                       synthetic scratch table) proves that even a raw ad-hoc SELECT against the
//                       table (bypassing the repository/stored-procedure layer entirely) returns
//                       zero rows for a session whose SESSION_CONTEXT('TenantId') does not match.
//                  This does not duplicate PlantSafety.IntegrationTests\Infrastructure\Persistence\
//                  TenantSessionContextRlsTests.cs (which proves the SESSION_CONTEXT propagation
//                  *mechanism* in the abstract, against a synthetic RlsTest.Widgets table): this
//                  file proves the *outcome* -- that two tenants' worth of DATA, created through
//                  the REAL production repository used by the REAL Accident aggregate, cannot leak
//                  across tenants -- and asserts on actual row counts, not merely "the query did
//                  not throw."
//                  90-row-level-security.sql itself cannot be applied verbatim here (it issues
//                  `USE [PlantSafety.CoreDB]` / `[PlantSafety.ArchiveDB]` / `[PlantSafety.DiagnosticsDB]`
//                  statements against named, single-purpose databases that do not exist as separate
//                  databases inside this shared Testcontainers instance -- the same USE-stripping
//                  convention TestDatabaseFactory already applies to every other Phase migration
//                  script it replays). The predicate function and FILTER/BLOCK policy created below
//                  are otherwise byte-for-byte the same mechanism (SESSION_CONTEXT('TenantId')
//                  equality, deny-by-default), applied to the one real table this suite needs.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Infrastructure.Persistence.Dapper;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Tenancy;

/// <summary>
/// Proves cross-tenant data leakage is prevented for the real <see cref="Accident"/> aggregate,
/// at both the application (repository) layer and the database (Row-Level-Security) layer.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CrossTenantDataLeakageTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private static readonly Guid TenantA = new("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = new("bbbbbbbb-2222-2222-2222-222222222222");

    /// <inheritdoc/>
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Removes the RLS policy this class installs on the shared
    /// [AccidentManagement].[Accident] table.
    /// </summary>
    /// <remarks>
    /// Every test in this project shares one database, and this class is the only one that turns
    /// RLS on. Leaving the policy behind leaked it into unrelated tests: once this class had run,
    /// any later test touching Accident over a raw connection with no SESSION_CONTEXT('TenantId')
    /// was silently filtered to zero rows on read, and rejected on write with "target object ...
    /// has a block predicate that conflicts with this operation". That is what broke
    /// AccidentConcurrencyAndAuditIntegrationTests, and it made the suite order-dependent. A test
    /// that mutates shared schema has to put it back.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync("""
            IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'AccidentTenantAccessPolicy')
                DROP SECURITY POLICY [Security].[AccidentTenantAccessPolicy];
            """);

        await conn.ExecuteAsync("""
            IF OBJECT_ID('Security.fn_AccidentTenantAccessPredicate', 'IF') IS NOT NULL
                DROP FUNCTION [Security].[fn_AccidentTenantAccessPredicate];
            """);
    }

    /// <summary>
    /// Applies the real production RLS mechanism (Security.fn_TenantAccessPredicate-equivalent
    /// FILTER + BLOCK policy, deny-by-default) directly to the real
    /// [AccidentManagement].[Accident] table. Idempotent -- safe to call once per test.
    /// </summary>
    private async Task EnsureAccidentRlsAsync()
    {
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
                EXEC('CREATE SCHEMA [Security]');
            """);

        // The policy must be dropped before the SCHEMABINDING predicate function it references
        // can be (re)created -- mirrors the real migration's own ordering requirement exactly.
        await conn.ExecuteAsync("""
            IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'AccidentTenantAccessPolicy')
                DROP SECURITY POLICY [Security].[AccidentTenantAccessPolicy];
            """);

        await conn.ExecuteAsync("""
            IF OBJECT_ID('Security.fn_AccidentTenantAccessPredicate', 'IF') IS NOT NULL
                DROP FUNCTION [Security].[fn_AccidentTenantAccessPredicate];
            """);

        // Byte-for-byte the same predicate shape as the real
        // Security.fn_TenantAccessPredicate(@TenantId) in
        // Persistence/Migrations/Phase9-Security/90-row-level-security.sql.
        await conn.ExecuteAsync("""
            EXEC('CREATE FUNCTION [Security].[fn_AccidentTenantAccessPredicate](@TenantId uniqueidentifier)
            RETURNS TABLE
            WITH SCHEMABINDING
            AS
            RETURN SELECT 1 AS fn_accessResult
            WHERE @TenantId = TRY_CAST(SESSION_CONTEXT(N''TenantId'') AS uniqueidentifier);');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'AccidentTenantAccessPolicy')
            BEGIN
                EXEC('CREATE SECURITY POLICY [Security].[AccidentTenantAccessPolicy]
                    ADD FILTER PREDICATE [Security].[fn_AccidentTenantAccessPredicate](TenantId) ON [AccidentManagement].[Accident],
                    ADD BLOCK PREDICATE [Security].[fn_AccidentTenantAccessPredicate](TenantId) ON [AccidentManagement].[Accident] AFTER INSERT,
                    ADD BLOCK PREDICATE [Security].[fn_AccidentTenantAccessPredicate](TenantId) ON [AccidentManagement].[Accident] AFTER UPDATE
                    WITH (STATE = ON);');
            END;
            """);
    }

    /// <summary>Creates a real, persisted Accident aggregate for the given tenant via the real repository.</summary>
    private static async Task<Accident> CreateAccidentAsync(IAccidentRepository repository, Guid tenantId, string title)
    {
        var ownerId = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(ownerId);

        var accident = Accident.Create(
            createdBy: ownerId,
            createdAtUtc: UtcTimestamp.Now(),
            correlationId: new CorrelationId(Guid.NewGuid()),
            causationId: new CausationId(Guid.NewGuid()),
            actor: actor,
            tenantId: tenantId,
            siteId: Guid.NewGuid(),
            accidentTitle: title,
            location: "Cross-Tenant Test Site",
            severity: AccidentSeverity.FirstAidOnly,
            injuredPersonCount: 0,
            occurredAtUtc: UtcTimestamp.Now(),
            ownerId: ownerId,
            reviewRequired: false);

        await repository.AddAsync(accident, CancellationToken.None);
        return accident;
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-ISO-001: Repository.GetByIdAsync never returns another tenant's Accident, even given the correct AccidentId")]
    public async Task GetByIdAsync_CrossTenantRequest_ReturnsNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();

        var tenantAAccident = await CreateAccidentAsync(repository, TenantA, "Tenant A's confidential accident");

        // Tenant B requests the exact AccidentId that belongs to Tenant A.
        var resultForTenantB = await repository.GetByIdAsync(tenantAAccident.AccidentId, TenantB, CancellationToken.None);

        resultForTenantB.Should().BeNull(
            "usp_Accident_GetById's WHERE clause requires TenantId = @TenantId -- Tenant B's claim must never match Tenant A's row, " +
            "even with the correct AccidentId in hand (e.g. guessed or leaked from another channel)");

        // Sanity: the owning tenant CAN still retrieve its own row (proves this is tenant-scoping,
        // not a general regression breaking every read).
        var resultForTenantA = await repository.GetByIdAsync(tenantAAccident.AccidentId, TenantA, CancellationToken.None);
        resultForTenantA.Should().NotBeNull("the legitimately owning tenant must still be able to read its own row");
        resultForTenantA!.TenantId.Should().Be(TenantA);
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-ISO-002: Real RLS policy on the real [AccidentManagement].[Accident] table denies ALL rows for a mismatched SESSION_CONTEXT, proving DB-layer defense-in-depth")]
    public async Task RawSelect_AgainstRealAccidentTable_DeniesCrossTenantRows_UnderRls()
    {
        await EnsureAccidentRlsAsync();

        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();

        var tenantAAccident = await CreateAccidentAsync(repository, TenantA, "Tenant A widget-shaped accident (RLS probe)");
        var tenantBAccident = await CreateAccidentAsync(repository, TenantB, "Tenant B widget-shaped accident (RLS probe)");

        // A raw, ad-hoc SELECT that completely bypasses the repository/stored-procedure layer --
        // simulating a rogue query, an ad-hoc reporting tool, or a future developer mistake that
        // forgets to scope by TenantId in application code. Session context is set to Tenant A.
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        await conn.SetTenantSessionContextAsync(TenantA);

        var visibleAccidentIds = (await conn.QueryAsync<Guid>(
            "SELECT AccidentId FROM [AccidentManagement].[Accident] WHERE AccidentId IN (@A, @B)",
            new { A = tenantAAccident.AccidentId.Value, B = tenantBAccident.AccidentId.Value })).ToList();

        visibleAccidentIds.Should().ContainSingle().Which.Should().Be(tenantAAccident.AccidentId.Value,
            "the real RLS FILTER predicate must hide Tenant B's row from a Tenant A SESSION_CONTEXT, " +
            "even for a raw ad-hoc query that never goes through the repository's WHERE-clause scoping");
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-ISO-003: usp_Accident_Update's existence check blocks a cross-tenant write even when the correct AccidentId is supplied")]
    public async Task UpdateAsync_CrossTenantAttempt_DoesNotMutateAnotherTenantsRow()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();

        var tenantAAccident = await CreateAccidentAsync(repository, TenantA, "Tenant A row a hijacker will attempt to mutate");

        // Simulate an attacker claiming Tenant B's context while somehow holding Tenant A's real
        // AccidentId (e.g. via IDOR/id-guessing). Tenant B's own GetByIdAsync already returns null
        // (proven by ISO-001); this test goes one step further and proves the actual
        // stored-procedure UPDATE path -- not just the read path -- also refuses to touch Tenant
        // A's row: usp_Accident_Update's own "row exists for this AccidentId AND TenantId" guard
        // THROWs (error 50004) rather than silently affecting zero rows, giving an even stronger
        // guarantee than a no-op would.
        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        Func<Task> act = async () => await conn.ExecuteAsync(
            "[AccidentManagement].[usp_Accident_Update]",
            new
            {
                AccidentId = tenantAAccident.AccidentId.Value,
                TenantId = TenantB, // the attacker's claimed tenant -- does not own this row
                Status = "SiteSecuringAndTriage",
                Category = "SiteSecuringAndTriage",
                SiteSecuredAndTriaged = true,
            },
            commandType: CommandType.StoredProcedure);

        await act.Should().ThrowAsync<SqlException>().Where(ex => ex.Number == 50004,
            "usp_Accident_Update's existence guard requires a row matching BOTH AccidentId AND TenantId; " +
            "Tenant B's claim must never match Tenant A's row, so the procedure must THROW rather than silently succeed");

        var afterAttempt = await repository.GetByIdAsync(tenantAAccident.AccidentId, TenantA, CancellationToken.None);
        afterAttempt.Should().NotBeNull();
        afterAttempt!.Status.Should().Be(AccidentStatus.Reported,
            "the cross-tenant update attempt must not have changed Tenant A's row at all");
    }
}
