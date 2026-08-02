// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.AccidentManagement
//  Layer:          IntegrationTests
//  File:           AccidentConcurrencyAndAuditIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 2 SQL Modernization coverage for [AccidentManagement].[Accident] /
//                  [AccidentManagement].[CorrectiveAction]'s newly added RowVersion columns and
//                  their ETag projections in usp_Accident_GetById / usp_Accident_GetAllActive /
//                  usp_CorrectiveAction_GetByAccidentId. The AccidentManagement.Accident domain
//                  aggregate has no ETag concept and IAccidentRepository does not expose one (see
//                  AccidentDapperRepository.AccidentRow.ETag remarks), so this coverage reads the
//                  real stored procedures directly against the Testcontainers SQL Server instance
//                  -- the same "grounded in real, verifiable schema" approach used by
//                  CorrectiveActionLifecycleIntegrationTests -- rather than inventing a domain-level
//                  ETag round-trip the write path doesn't actually support yet (schema-only,
//                  additive-only, matching the Analytics/Compliance/Operational precedent).
//                  Also confirms usp_Accident_Insert's/usp_Accident_Update's pre-existing
//                  TRY/CATCH + AuditLog + ErrorLog pattern is genuinely wired up (no prior test in
//                  this module asserted against AuditLog/ErrorLog directly).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.AccidentManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AccidentConcurrencyAndAuditIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Accident_ETag_ShouldBeStableAcrossReadsAndAdvanceOnUpdate()
    {
        var scope = Factory.Services.CreateScope();
        Guid accidentId;
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var accident = Accident.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                accidentTitle: "ETag stability probe",
                location: "Warehouse 3",
                severity: AccidentSeverity.FirstAidOnly,
                injuredPersonCount: 0,
                occurredAtUtc: UtcTimestamp.Now(),
                ownerId: ownerId,
                reviewRequired: false);
            accidentId = accident.AccidentId.Value;

            await repository.AddAsync(accident, CancellationToken.None);

            // ---- Two unmodified reads via the real usp_Accident_GetById must yield the
            // identical ETag -- the ROWVERSION-backed value must not advance on read. ----
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var firstEtag = await GetAccidentETagAsync(conn, accidentId);
            var secondEtag = await GetAccidentETagAsync(conn, accidentId);

            firstEtag.Should().NotBeNullOrWhiteSpace("RowVersion must populate on insert");
            firstEtag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");
            secondEtag.Should().Be(firstEtag, "reading the same unmodified row twice must not advance the ROWVERSION-backed ETag");

            // ---- A genuine UPDATE through the real repository/usp_Accident_Update must
            // advance the RowVersion, and therefore the projected ETag. ----
            accident.ConfirmSiteSecuredAndTriaged();
            await repository.UpdateAsync(accident, eTag: null, CancellationToken.None);

            var thirdEtag = await GetAccidentETagAsync(conn, accidentId);
            thirdEtag.Should().NotBe(firstEtag, "usp_Accident_Update modifying the row must advance the ROWVERSION-backed ETag");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CorrectiveAction_ETag_ShouldAdvanceOnUpsertUpdate()
    {
        var scope = Factory.Services.CreateScope();
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var investigatorId = UserId.FromGuid(Guid.NewGuid());
            var responsiblePersonId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var accident = Accident.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                accidentTitle: "CorrectiveAction ETag probe",
                location: "Warehouse 4",
                severity: AccidentSeverity.FirstAidOnly,
                injuredPersonCount: 1,
                occurredAtUtc: UtcTimestamp.Now(),
                ownerId: ownerId,
                reviewRequired: true);

            await repository.AddAsync(accident, CancellationToken.None);

            accident.ConfirmSiteSecuredAndTriaged();
            accident.AssignInvestigator(investigatorId, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            accident.RecordRootCause(
                RootCauseCategory.Environmental, "Root cause for ETag probe.",
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            var correctiveAction = accident.CreateCorrectiveAction(
                CorrectiveActionCategory.AdministrativeControls,
                "CAPA for ETag probe.",
                responsiblePersonId,
                UtcTimestamp.From(DateTime.UtcNow.AddDays(7)),
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            await repository.UpdateAsync(accident, eTag: null, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var firstEtag = await GetCorrectiveActionETagAsync(conn, correctiveAction.CorrectiveActionId.Value);
            firstEtag.Should().NotBeNullOrWhiteSpace("RowVersion must populate on the CorrectiveAction insert");
            firstEtag.Should().StartWith("0x");

            var afterCreate = await repository.GetByIdAsync(accident.AccidentId, TenantId, CancellationToken.None);
            afterCreate!.CompleteCorrectiveAction(
                correctiveAction.CorrectiveActionId, "Evidence for ETag probe.",
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            await repository.UpdateAsync(afterCreate, eTag: null, CancellationToken.None);

            var secondEtag = await GetCorrectiveActionETagAsync(conn, correctiveAction.CorrectiveActionId.Value);
            secondEtag.Should().NotBe(firstEtag, "usp_CorrectiveAction_Upsert's UPDATE branch modifying the row must advance the ROWVERSION-backed ETag");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task AccidentInsertAndUpdate_ShouldWriteAuditLogRows()
    {
        var scope = Factory.Services.CreateScope();
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IAccidentRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var accident = Accident.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                accidentTitle: "AuditLog coverage probe",
                location: "Warehouse 5",
                severity: AccidentSeverity.FirstAidOnly,
                injuredPersonCount: 0,
                occurredAtUtc: UtcTimestamp.Now(),
                ownerId: ownerId,
                reviewRequired: false);
            var accidentId = accident.AccidentId.Value;

            await repository.AddAsync(accident, CancellationToken.None);

            accident.ConfirmSiteSecuredAndTriaged();
            await repository.UpdateAsync(accident, eTag: null, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);

            var insertAuditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Accident", Operation = "INSERT", Pattern = $"%AccidentId={accidentId}%" });
            insertAuditCount.Should().BeGreaterThan(0,
                "usp_Accident_Insert's canonical AuditLog insert must have recorded this Create");

            var updateAuditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Accident", Operation = "UPDATE", Pattern = $"%AccidentId={accidentId}%" });
            updateAuditCount.Should().BeGreaterThan(0,
                "usp_Accident_Update's canonical AuditLog insert must have recorded this Update");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateAccident_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [AccidentManagement].[usp_Accident_Insert] directly, twice, with the same @AccidentId.
        // Confirms the pre-existing procedure's THROW still re-raises the exception and that the
        // CATCH block's ErrorLog insert fires -- matching the same pattern used by
        // Compliance.MocWritePathIntegrationTests.CreateMoc_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog.
        var accidentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteInsertAsync()
        {
            await using var cmd = new SqlCommand("[AccidentManagement].[usp_Accident_Insert]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AccidentId", accidentId);
            cmd.Parameters.AddWithValue("@TenantId", TenantId);
            cmd.Parameters.AddWithValue("@SiteId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Status", "Reported");
            cmd.Parameters.AddWithValue("@AccidentTitle", "Forced duplicate-key failure test");
            cmd.Parameters.AddWithValue("@Location", "Warehouse 6");
            cmd.Parameters.AddWithValue("@Severity", "FirstAidOnly");
            cmd.Parameters.AddWithValue("@InjuredPersonCount", 0);
            cmd.Parameters.AddWithValue("@OccurredAtUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@Category", "SiteSecuringAndTriage");
            cmd.Parameters.AddWithValue("@SiteSecuredAndTriaged", false);
            cmd.Parameters.AddWithValue("@RegulatoryFilingRequired", false);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            cmd.Parameters.AddWithValue("@ReviewRequired", true);
            cmd.Parameters.AddWithValue("@CreatedAtUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@CreatedBy", ownerId);

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteInsertAsync();

        var act = ExecuteInsertAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_Accident_Insert call with a duplicate @AccidentId must violate PK_Accident and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Accident_Insert", Pattern = $"%AccidentId={accidentId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Accident_Insert's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }

    private static async Task<string?> GetAccidentETagAsync(SqlConnection conn, Guid accidentId)
    {
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [AccidentManagement].[Accident] WHERE AccidentId = @AccidentId",
            new { AccidentId = accidentId });
    }

    private static async Task<string?> GetCorrectiveActionETagAsync(SqlConnection conn, Guid correctiveActionId)
    {
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [AccidentManagement].[CorrectiveAction] WHERE CorrectiveActionId = @CorrectiveActionId",
            new { CorrectiveActionId = correctiveActionId });
    }
}
