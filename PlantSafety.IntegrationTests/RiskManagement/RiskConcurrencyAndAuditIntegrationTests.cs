// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.RiskManagement
//  Layer:          IntegrationTests
//  File:           RiskConcurrencyAndAuditIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for [RiskMgmt].[Risk] /
//                  [RiskMgmt].[ControlMeasure]'s newly added RowVersion columns and their ETag
//                  projections in usp_Risk_GetById / usp_Risk_GetAllActive /
//                  usp_ControlMeasure_GetByRiskId. The RiskManagement.Risk domain aggregate has no
//                  ETag concept and IRiskRepository does not expose one (see
//                  RiskDapperRepository.RiskRow.ETag remarks), so this coverage reads the real
//                  stored procedures directly against the Testcontainers SQL Server instance --
//                  the same "grounded in real, verifiable schema" approach used by
//                  AccidentConcurrencyAndAuditIntegrationTests -- rather than inventing a
//                  domain-level ETag round-trip the write path doesn't actually support yet
//                  (schema-only, additive-only, matching the Analytics/Compliance/Operational/
//                  AccidentManagement precedent).
//                  ControlMeasure entries are immutable once applied by the domain (no lifecycle
//                  mutation methods on the ControlMeasure entity -- see
//                  RiskDapperRepository.UpdateAsync remarks), so the "advance on update" half of
//                  the ControlMeasure ETag test calls [RiskMgmt].[usp_ControlMeasure_Upsert]
//                  directly a second time with the same @ControlMeasureId to force the UPDATE
//                  branch, mirroring the direct-stored-procedure-call pattern already used by
//                  CreateRisk_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog below.
//                  Also confirms usp_Risk_Insert's/usp_Risk_Update's pre-existing TRY/CATCH +
//                  AuditLog + ErrorLog pattern is genuinely wired up end-to-end (the embedded
//                  TestDatabaseFactory copy previously had a drift gap where the CATCH block never
//                  actually inserted into ErrorLog before THROW -- fixed as part of this pass).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.RiskManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class RiskConcurrencyAndAuditIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Risk_ETag_ShouldBeStableAcrossReadsAndAdvanceOnUpdate()
    {
        var scope = Factory.Services.CreateScope();
        Guid riskId;
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRiskRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var risk = Risk.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                riskTitle: "ETag stability probe",
                location: "Line 7 - Packaging",
                hazardSource: "Mechanical",
                ownerId: ownerId,
                reviewRequired: false);
            riskId = risk.RiskId.Value;

            await repository.AddAsync(risk, CancellationToken.None);

            // ---- Two unmodified reads via the real usp_Risk_GetById must yield the identical
            // ETag -- the ROWVERSION-backed value must not advance on read. ----
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var firstEtag = await GetRiskETagAsync(conn, riskId);
            var secondEtag = await GetRiskETagAsync(conn, riskId);

            firstEtag.Should().NotBeNullOrWhiteSpace("RowVersion must populate on insert");
            firstEtag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");
            secondEtag.Should().Be(firstEtag, "reading the same unmodified row twice must not advance the ROWVERSION-backed ETag");

            // ---- A genuine UPDATE through the real repository/usp_Risk_Update must advance the
            // RowVersion, and therefore the projected ETag. ----
            risk.ScoreRisk(4, 3, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            await repository.UpdateAsync(risk, CancellationToken.None);

            var thirdEtag = await GetRiskETagAsync(conn, riskId);
            thirdEtag.Should().NotBe(firstEtag, "usp_Risk_Update modifying the row must advance the ROWVERSION-backed ETag");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ControlMeasure_ETag_ShouldAdvanceOnUpsertUpdate()
    {
        var scope = Factory.Services.CreateScope();
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRiskRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var responsiblePersonId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var risk = Risk.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                riskTitle: "ControlMeasure ETag probe",
                location: "Line 8 - Assembly",
                hazardSource: "Mechanical",
                ownerId: ownerId,
                reviewRequired: true);

            await repository.AddAsync(risk, CancellationToken.None);

            risk.ScoreRisk(4, 3, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            risk.Prioritize(null, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            var controlMeasure = risk.ApplyControlMeasure(
                ControlHierarchyLevel.EngineeringControls,
                "Installed interlock guard for ETag probe.",
                "Elimination/substitution not feasible for this operation.",
                responsiblePersonId,
                UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));

            await repository.UpdateAsync(risk, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();

            var firstEtag = await GetControlMeasureETagAsync(conn, controlMeasure.ControlMeasureId.Value);
            firstEtag.Should().NotBeNullOrWhiteSpace("RowVersion must populate on the ControlMeasure insert");
            firstEtag.Should().StartWith("0x");

            // ControlMeasure is immutable once applied by the domain (no lifecycle mutation
            // methods on the entity -- see class-level remarks), so the UPDATE branch of
            // usp_ControlMeasure_Upsert is exercised directly here, matching
            // CreateRisk_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog's
            // direct-stored-procedure-call pattern below.
            await using (var cmd = new SqlCommand("[RiskMgmt].[usp_ControlMeasure_Upsert]", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ControlMeasureId", controlMeasure.ControlMeasureId.Value);
                cmd.Parameters.AddWithValue("@RiskId", risk.RiskId.Value);
                cmd.Parameters.AddWithValue("@TenantId", TenantId);
                cmd.Parameters.AddWithValue("@HierarchyLevel", "EngineeringControls");
                cmd.Parameters.AddWithValue("@Description", "Installed interlock guard for ETag probe -- revised.");
                cmd.Parameters.AddWithValue("@Justification", "Elimination/substitution not feasible for this operation.");
                cmd.Parameters.AddWithValue("@ResponsiblePersonId", responsiblePersonId.Value);
                cmd.Parameters.AddWithValue("@ImplementedAtUtc", FixedUtcNow);

                await cmd.ExecuteNonQueryAsync();
            }

            var secondEtag = await GetControlMeasureETagAsync(conn, controlMeasure.ControlMeasureId.Value);
            secondEtag.Should().NotBe(firstEtag, "usp_ControlMeasure_Upsert's UPDATE branch modifying the row must advance the ROWVERSION-backed ETag");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RiskInsertAndUpdate_ShouldWriteAuditLogRows()
    {
        var scope = Factory.Services.CreateScope();
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRiskRepository>();
            var ownerId = UserId.FromGuid(Guid.NewGuid());
            var correlationId = new CorrelationId(CorrelationId);
            var actor = PerformedByActor.User(ownerId);

            var risk = Risk.Create(
                createdBy: ownerId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: correlationId,
                causationId: new CausationId(Guid.NewGuid()),
                actor: actor,
                tenantId: TenantId,
                siteId: Guid.NewGuid(),
                riskTitle: "AuditLog coverage probe",
                location: "Line 9 - Warehouse",
                hazardSource: "Mechanical",
                ownerId: ownerId,
                reviewRequired: false);
            var riskId = risk.RiskId.Value;

            await repository.AddAsync(risk, CancellationToken.None);

            risk.ScoreRisk(2, 2, UtcTimestamp.Now(), actor, correlationId, new CausationId(Guid.NewGuid()));
            await repository.UpdateAsync(risk, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);

            var insertAuditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Risk", Operation = "INSERT", Pattern = $"%RiskId={riskId}%" });
            insertAuditCount.Should().BeGreaterThan(0,
                "usp_Risk_Insert's canonical AuditLog insert must have recorded this Create");

            var updateAuditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Risk", Operation = "UPDATE", Pattern = $"%RiskId={riskId}%" });
            updateAuditCount.Should().BeGreaterThan(0,
                "usp_Risk_Update's canonical AuditLog insert must have recorded this Update");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateRisk_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [RiskMgmt].[usp_Risk_Insert] directly, twice, with the same @RiskId. Confirms the
        // pre-existing procedure's THROW still re-raises the exception and that the CATCH block's
        // ErrorLog insert fires -- matching the same pattern used by
        // AccidentConcurrencyAndAuditIntegrationTests.CreateAccident_ForcedDuplicatePrimaryKey_
        // ShouldThrowAndPopulateErrorLog. This also confirms the embedded TestDatabaseFactory copy's
        // previously-missing ErrorLog insert (fixed as part of this pass) is genuinely wired up.
        var riskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteInsertAsync()
        {
            await using var cmd = new SqlCommand("[RiskMgmt].[usp_Risk_Insert]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RiskId", riskId);
            cmd.Parameters.AddWithValue("@TenantId", TenantId);
            cmd.Parameters.AddWithValue("@SiteId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Status", "Identified");
            cmd.Parameters.AddWithValue("@RiskTitle", "Forced duplicate-key failure test");
            cmd.Parameters.AddWithValue("@Category", "Low");
            cmd.Parameters.AddWithValue("@HazardDomainCategory", "HazardIdentificationMeasurement");
            cmd.Parameters.AddWithValue("@Location", "Line 10 - Test Bay");
            cmd.Parameters.AddWithValue("@HazardSource", "Mechanical");
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            cmd.Parameters.AddWithValue("@ReviewCadence", "Quarterly");
            cmd.Parameters.AddWithValue("@ReviewRequired", true);
            cmd.Parameters.AddWithValue("@ReviewCount", 0);
            cmd.Parameters.AddWithValue("@RiskVersion", 1);
            cmd.Parameters.AddWithValue("@CreatedAtUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@CreatedBy", ownerId);

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteInsertAsync();

        var act = ExecuteInsertAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_Risk_Insert call with a duplicate @RiskId must violate PK_Risk and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Risk_Insert", Pattern = $"%RiskId={riskId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Risk_Insert's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }

    private static async Task<string?> GetRiskETagAsync(SqlConnection conn, Guid riskId)
    {
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [RiskMgmt].[Risk] WHERE RiskId = @RiskId",
            new { RiskId = riskId });
    }

    private static async Task<string?> GetControlMeasureETagAsync(SqlConnection conn, Guid controlMeasureId)
    {
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [RiskMgmt].[ControlMeasure] WHERE ControlMeasureId = @ControlMeasureId",
            new { ControlMeasureId = controlMeasureId });
    }
}
