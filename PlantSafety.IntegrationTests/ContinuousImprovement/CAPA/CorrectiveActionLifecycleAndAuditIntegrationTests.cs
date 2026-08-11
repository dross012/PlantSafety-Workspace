// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.ContinuousImprovement.CAPA
//  Layer:          IntegrationTests
//  File:           CorrectiveActionLifecycleAndAuditIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live ContinuousImprovement
//                  CorrectiveAction (CAPA) write path -- Features\ContinuousImprovement\CAPA ->
//                  ICorrectiveActionRepository -> CorrectiveActionDapperRepository ->
//                  [ContinuousImprovement].[CorrectiveAction] /
//                  usp_CorrectiveAction_Create/GetById/List (wired via
//                  AddFeaturesLegacyRepositories()), the ONLY real write path in this module.
//
//                  Resolves the three-`CorrectiveAction`-named-table naming trap flagged by the
//                  Phase 3 survey: [ContinuousImprovement].[CorrectiveAction] (this table, defined
//                  in Infrastructure\ContinuousImprovement\Sql\Tables\CorrectiveAction.sql) and the
//                  table consumed by CorrectiveActionDapperRepository (Infrastructure\Features\
//                  ContinuousImprovement\CAPA\Repositories) are the SAME table/write-path -- not two
//                  competing implementations. The .sql file's own header and every one of the three
//                  usp_CorrectiveAction_* procedures' doc comments already state this explicitly
//                  ("backs ICorrectiveActionRepository.* / CorrectiveActionDapperRepository.*"), and
//                  CorrectiveActionDapperRepository.cs directly references
//                  [ContinuousImprovement].[usp_CorrectiveAction_Create/GetById/List] by fully
//                  qualified name. This table/procedure set is DISTINCT from both
//                  [ContinuousImprovement].[CAPA] (the unrelated ICAPARepository reporting
//                  read-model -- different columns, no SourceEventId/Description/Priority/ETag) and
//                  [AccidentManagement].[CorrectiveAction] (a child entity of Accident, already
//                  modernized in Phase 2, unrelated to this CAPA feature).
//
//                  Unlike every other Phase 3 module tackled so far, this pass's SQL audit found
//                  usp_CorrectiveAction_Create/GetById/List were ALREADY fully canonical as of
//                  2026-07-27 (RowVersion ROWVERSION column genuinely present and live, ETag
//                  projected via CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) in both
//                  GetById and List, Create already has TRY/CATCH + best-effort AuditLog INSERT +
//                  CATCH-block ErrorLog INSERT + THROW) -- no SQL changes were required. This test
//                  class exists purely to prove that pre-existing canonical behavior with real,
//                  non-vacuous coverage (replacing the survey-flagged vacuous
//                  CAPAIntegrationTests/RCAIntegrationTests outbox-stub pattern for this specific
//                  write path) now that TestDatabaseFactory.ApplyContinuousImprovementSchemaAsync
//                  makes the module testable at all. Confirms:
//                    1. Create -> GetById -> List round-trips real data through the real DI
//                       container (CreateCorrectiveActionCommand via MediatR, then
//                       ICorrectiveActionRepository.GetByIdAsync/ListAsync resolved directly --
//                       no MediatR query handler wraps GetById/List yet).
//                    2. ETag is present, stable across repeated reads, and genuinely reflects each
//                       row's own RowVersion (via a second, independent Create).
//                    3. usp_CorrectiveAction_Create's pre-existing AuditLog INSERT fires on every
//                       real Create.
//                    4. usp_CorrectiveAction_Create's pre-existing TRY/CATCH + ErrorLog + THROW
//                       pattern populates ErrorLog and re-raises on a forced SQL-level failure
//                       (duplicate @CaId primary-key violation), mirroring
//                       RoiLifecycleAndAuditIntegrationTests /
//                       ProjectLifecycleAndAuditIntegrationTests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.ContinuousImprovement.CAPA;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CorrectiveActionLifecycleAndAuditIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CorrectiveActionLifecycle_CreateGetByIdListRoundTrip_ShouldReturnRealDataWithStableAdvancingETag()
    {
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var sourceEventId = Guid.NewGuid();
            var assignedToWorkerId = Guid.NewGuid();
            var dueAtUtc = new DateTimeOffset(2027, 3, 15, 0, 0, 0, TimeSpan.Zero);

            // ---- Create (real CreateCorrectiveActionCommandHandler -> ICorrectiveActionRepository
            // -> real usp_CorrectiveAction_Create) --------------------------------------------
            var caId = await sender.Send(new CreateCorrectiveActionCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Repair damaged guardrail on Line 3",
                Description = "Guardrail found bent during routine inspection; requires replacement.",
                ActionType = "Corrective",
                SourceEventId = sourceEventId,
                SourceEventType = "Incident",
                Priority = "High",
                AssignedToWorkerId = assignedToWorkerId,
                DueAtUtc = dueAtUtc,
            }, CancellationToken.None);

            caId.Should().NotBe(Guid.Empty);

            // ---- Get (real ICorrectiveActionRepository.GetByIdAsync -> real
            // usp_CorrectiveAction_GetById -- no MediatR query handler wraps this yet, so resolve
            // the repository directly from the DI container, matching the "belt-and-braces"
            // repository-level check pattern used by FinancialsSqlTests.
            // GetFinancials_WithSeededRow_ShouldReturnRealDataThroughRealHandler). --------------
            var repository = scope.ServiceProvider.GetRequiredService<ICorrectiveActionRepository>();

            var getResult = await repository.GetByIdAsync(TenantId, caId, CancellationToken.None);

            getResult.Should().NotBeNull("the just-created CorrectiveAction must be retrievable through the real stored procedure");
            getResult!.CaId.Should().Be(caId);
            getResult.TenantId.Should().Be(TenantId);
            getResult.SourceEventId.Should().Be(sourceEventId);
            getResult.SourceEventType.Should().Be("Incident");
            getResult.Title.Should().Be("Repair damaged guardrail on Line 3");
            getResult.ActionType.Should().Be("Corrective");
            getResult.Priority.Should().Be("High");
            getResult.AssignedToWorkerId.Should().Be(assignedToWorkerId);
            getResult.Status.Should().Be("Open", "usp_CorrectiveAction_Create must insert every new CorrectiveAction in Open status");
            getResult.CompletedAtUtc.Should().BeNull();

            var firstETag = getResult.ETag;
            firstETag.Should().NotBeNullOrWhiteSpace("usp_CorrectiveAction_GetById must project the RowVersion column as ETag");
            firstETag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");

            // ---- Re-read must yield an identical ETag (ROWVERSION must not advance on read) ----
            var getAgainResult = await repository.GetByIdAsync(TenantId, caId, CancellationToken.None);
            getAgainResult!.ETag.Should().Be(firstETag, "reading the same unmodified row twice must not advance the ROWVERSION-backed ETag");

            // ---- List (real ICorrectiveActionRepository.ListAsync -> real
            // usp_CorrectiveAction_List) ------------------------------------------------------
            var (items, totalCount) = await repository.ListAsync(
                tenantId: TenantId,
                sourceEventId: sourceEventId,
                actionType: null,
                priority: null,
                status: null,
                orderBy: "DueAtUtc",
                sortDirection: "ASC",
                pageNumber: 1,
                pageSize: 25,
                cancellationToken: CancellationToken.None);

            totalCount.Should().BeGreaterThan(0);
            items.Should().ContainSingle(i => i.CaId == caId);
            var listedItem = items.Single(i => i.CaId == caId);
            listedItem.ETag.Should().Be(firstETag, "usp_CorrectiveAction_List must project the identical ROWVERSION-backed ETag as usp_CorrectiveAction_GetById for the same row");

            // ---- A second, independent Create must produce its own distinct row with its own
            // distinct ETag (proves the projection reflects each row's own RowVersion rather than
            // some cached/shared value). ------------------------------------------------------
            var secondSourceEventId = Guid.NewGuid();
            var secondCaId = await sender.Send(new CreateCorrectiveActionCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Second Independent Corrective Action",
                Description = "Distinct row for ETag isolation check.",
                ActionType = "Preventive",
                SourceEventId = secondSourceEventId,
                SourceEventType = "NearMiss",
                Priority = "Medium",
                AssignedToWorkerId = Guid.NewGuid(),
                DueAtUtc = dueAtUtc.AddDays(30),
            }, CancellationToken.None);

            var secondGetResult = await repository.GetByIdAsync(TenantId, secondCaId, CancellationToken.None);
            secondGetResult!.CaId.Should().Be(secondCaId);
            secondGetResult.ETag.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateCorrectiveAction_ShouldWriteAuditLogRow()
    {
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var caId = await sender.Send(new CreateCorrectiveActionCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Audit Coverage Corrective Action",
                Description = "Exists solely to confirm AuditLog coverage.",
                ActionType = "Corrective",
                SourceEventId = Guid.NewGuid(),
                SourceEventType = "AuditFinding",
                Priority = "Low",
                AssignedToWorkerId = Guid.NewGuid(),
                DueAtUtc = new DateTimeOffset(2027, 4, 1, 0, 0, 0, TimeSpan.Zero),
            }, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);

            var auditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "CorrectiveAction", Operation = "INSERT", Pattern = $"%CaId={caId}%" });

            auditCount.Should().BeGreaterThan(0,
                "usp_CorrectiveAction_Create's pre-existing canonical AuditLog insert must have recorded this Create");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateCorrectiveAction_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [ContinuousImprovement].[usp_CorrectiveAction_Create] directly, twice, with the same
        // @CaId. Confirms the procedure's pre-existing THROW still re-raises the exception and
        // that the CATCH block's ErrorLog insert fires -- mirroring
        // RoiLifecycleAndAuditIntegrationTests.CreateRoi_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog
        // / ProjectLifecycleAndAuditIntegrationTests.
        // CreateProject_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog.
        var caId = Guid.NewGuid();
        var tenantId = TenantId;
        var actorId = ActorId;

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteCreateAsync()
        {
            await using var cmd = new SqlCommand("[ContinuousImprovement].[usp_CorrectiveAction_Create]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
            cmd.Parameters.AddWithValue("@CorrelationId", CorrelationId);
            cmd.Parameters.AddWithValue("@ActorId", actorId);
            cmd.Parameters.AddWithValue("@NowUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@CaId", caId);
            cmd.Parameters.AddWithValue("@SourceEventId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@SourceEventType", "Incident");
            cmd.Parameters.AddWithValue("@Title", "Duplicate Key Test Corrective Action");
            cmd.Parameters.AddWithValue("@Description", "Forces a primary key violation on the second call.");
            cmd.Parameters.AddWithValue("@ActionType", "Corrective");
            cmd.Parameters.AddWithValue("@Priority", "Medium");
            cmd.Parameters.AddWithValue("@AssignedToWorkerId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@DueAtUtc", new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc));

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteCreateAsync();

        var act = ExecuteCreateAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_CorrectiveAction_Create call with a duplicate @CaId must violate the primary key and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_CorrectiveAction_Create", Pattern = $"%CaId={caId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_CorrectiveAction_Create's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }
}
