// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Compliance.ManagementOfChange
//  Layer:          IntegrationTests
//  File:           MocWritePathIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Exercises CreateMocCommand -- one of only three real Compliance write actions
//                  (see docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md
//                  Sec 1.2) -- through the real MediatR pipeline, the real MocDapperRepository, and
//                  the real [Compliance].[Moc] table + usp_Moc_Create/usp_Moc_Approve/
//                  usp_Moc_GetById stored procedures applied to the Testcontainers SQL Server
//                  instance (see Setup\TestDatabaseFactory.ApplyComplianceSchemaAsync). Neither the
//                  Moc table nor its stored procedures existed anywhere in the codebase before this
//                  pass, even though MocDapperRepository referenced them by name -- CreateMocCommand
//                  could never have succeeded against a real database until now. There is no
//                  GetMocByIdQuery wired to any handler, so persistence is asserted via a direct SQL
//                  read against [Compliance].[Moc] rather than through the MediatR pipeline.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Features.Compliance.Commands.ManagementOfChange.Create;
using PlantSafety.Application.Features.Compliance.Handlers.ManagementOfChange;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Compliance.ManagementOfChange;

/// <summary>
/// Exercises <c>CreateMocCommand</c> through the real MediatR pipeline, the real
/// <c>MocDapperRepository</c>, and the real <c>[Compliance].[Moc]</c> table + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class MocWritePathIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateMoc_ShouldPersistToRealMocTable()
    {
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var initiatorId = Guid.NewGuid();

            var createCommand = new CreateMocCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Title = "Replace relief valve on Reactor 4",
                Description = "Swap the pressure relief valve for a higher-rated unit ahead of the process change.",
                ChangeType = "Permanent",
                RiskLevel = "High",
                InitiatorId = initiatorId,
                ProposedStartUtc = DateTimeOffset.UtcNow.AddDays(7),
                ProposedEndUtc = DateTimeOffset.UtcNow.AddDays(8),
            };

            var mocId = await sender.Send(createCommand, CancellationToken.None);
            mocId.Should().NotBe(Guid.Empty);

            await using var conn = new SqlConnection(ConnectionString);
            var row = await conn.QuerySingleOrDefaultAsync(
                @"SELECT Title, Description, ChangeType, RiskLevel, Status, TenantId, SiteId, InitiatorId
                  FROM [Compliance].[Moc]
                  WHERE MocId = @MocId",
                new { MocId = mocId });

            ((object?)row).Should().NotBeNull("CreateMocCommand must persist a real row via usp_Moc_Create");
            ((string)row.Title).Should().Be(createCommand.Title);
            ((string)row.ChangeType).Should().Be("Permanent");
            ((string)row.RiskLevel).Should().Be("High");
            ((string)row.Status).Should().Be("Draft", "usp_Moc_Create defaults new MOCs to Draft status");
            ((Guid)row.TenantId).Should().Be(TenantId);
            ((Guid)row.SiteId).Should().Be(siteId);
            ((Guid)row.InitiatorId).Should().Be(initiatorId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ApproveMoc_WithValidETag_ShouldTransitionToApproved()
    {
        // IMocRepository.ApproveAsync/usp_Moc_Approve are exercised directly against the
        // Dapper repository (no ApproveMocCommand/handler is currently wired to MediatR -- see
        // the blueprint's "only three real write commands" finding, none of which is a MOC
        // approval). This still proves the real stored procedure, ROWVERSION-backed ETag guard,
        // and DapperCommandExecutor.ExecuteScalarAsync rowcount plumbing all work end-to-end.
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var mocId = await sender.Send(new CreateMocCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Temporary bypass of interlock during commissioning",
                Description = "Bypass the E-stop interlock for 48 hours during commissioning tests, under continuous supervision.",
                ChangeType = "Temporary",
                RiskLevel = "Medium",
                InitiatorId = Guid.NewGuid(),
                ProposedStartUtc = DateTimeOffset.UtcNow.AddDays(1),
                ProposedEndUtc = DateTimeOffset.UtcNow.AddDays(3),
            }, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);
            var eTag = await conn.ExecuteScalarAsync<string>(
                "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [Compliance].[Moc] WHERE MocId = @MocId",
                new { MocId = mocId });

            var (repoScope, mocRepository) = Factory.Services.GetScopedService<IMocRepository>();

            await mocRepository.ApproveAsync(
                tenantId: TenantId,
                mocId: mocId,
                approvalNotes: "Approved by Safety Committee -- conditions attached.",
                eTag: eTag!,
                actorId: ActorId,
                correlationId: CorrelationId,
                cancellationToken: CancellationToken.None);

            var statusAfterApprove = await conn.ExecuteScalarAsync<string>(
                "SELECT Status FROM [Compliance].[Moc] WHERE MocId = @MocId", new { MocId = mocId });
            statusAfterApprove.Should().Be("Approved");

            // Stale ETag must be rejected via the ExecuteScalarAsync rowcount check.
            var act = async () => await mocRepository.ApproveAsync(
                tenantId: TenantId,
                mocId: mocId,
                approvalNotes: "Second approval attempt with the now-stale ETag.",
                eTag: eTag!,
                actorId: ActorId,
                correlationId: CorrelationId,
                cancellationToken: CancellationToken.None);

            await act.Should().ThrowAsync<Exception>(
                "a stale ETag must be rejected by usp_Moc_Approve's ROWVERSION match and surfaced as a concurrency failure");

            repoScope.Dispose();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateMoc_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [Compliance].[usp_Moc_Create] directly, twice, with the same @MocId. Confirms the
        // modernized procedure's THROW still re-raises the exception and that the CATCH block's
        // ErrorLog insert fires.
        var mocId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteCreateAsync()
        {
            await using var cmd = new SqlCommand("[Compliance].[usp_Moc_Create]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", TenantId);
            cmd.Parameters.AddWithValue("@CorrelationId", CorrelationId);
            cmd.Parameters.AddWithValue("@ActorId", ActorId);
            cmd.Parameters.AddWithValue("@NowUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@MocId", mocId);
            cmd.Parameters.AddWithValue("@SiteId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Title", "Forced duplicate-key failure test");
            cmd.Parameters.AddWithValue("@Description", "Second insert with the same MocId must violate the primary key.");
            cmd.Parameters.AddWithValue("@ChangeType", "Temporary");
            cmd.Parameters.AddWithValue("@RiskLevel", "Low");
            cmd.Parameters.AddWithValue("@InitiatorId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@ProposedStartUtc", DateTimeOffset.UtcNow.AddDays(1));
            cmd.Parameters.AddWithValue("@ProposedEndUtc", DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteCreateAsync();

        var act = ExecuteCreateAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_Moc_Create call with a duplicate @MocId must violate PK_Moc and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Moc_Create", Pattern = $"%MocId={mocId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Moc_Create's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }

    [Fact]
    public async Task ApproveMoc_ForcedNotNullViolation_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (NOT NULL constraint violation on LastModifiedAtUtc)
        // by calling [Compliance].[usp_Moc_Approve] directly with @NowUtc = NULL against a Moc row
        // that genuinely exists and whose ETag genuinely matches (so the UPDATE actually reaches
        // the NOT NULL column, rather than matching zero rows on a stale/incorrect ETag).
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        Guid mocId;
        try
        {
            mocId = await sender.Send(new CreateMocCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Forced NOT NULL failure test",
                Description = "Approve call with a NULL @NowUtc must violate LastModifiedAtUtc's NOT NULL constraint.",
                ChangeType = "Temporary",
                RiskLevel = "Low",
                InitiatorId = Guid.NewGuid(),
                ProposedStartUtc = DateTimeOffset.UtcNow.AddDays(1),
                ProposedEndUtc = DateTimeOffset.UtcNow.AddDays(2),
            }, CancellationToken.None);
        }
        finally
        {
            scope.Dispose();
        }

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var eTag = await conn.ExecuteScalarAsync<string>(
            "SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) FROM [Compliance].[Moc] WHERE MocId = @MocId",
            new { MocId = mocId });

        await using (var cmd = new SqlCommand("[Compliance].[usp_Moc_Approve]", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", TenantId);
            cmd.Parameters.AddWithValue("@CorrelationId", CorrelationId);
            cmd.Parameters.AddWithValue("@ActorId", ActorId);
            cmd.Parameters.AddWithValue("@NowUtc", DBNull.Value);
            cmd.Parameters.AddWithValue("@MocId", mocId);
            cmd.Parameters.AddWithValue("@ApprovalNotes", DBNull.Value);
            cmd.Parameters.AddWithValue("@ETag", eTag!);

            var act = async () => await cmd.ExecuteScalarAsync();

            await act.Should().ThrowAsync<SqlException>(
                "a NULL @NowUtc must violate Moc.LastModifiedAtUtc's NOT NULL constraint and THROW must re-raise it");
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Moc_Approve", Pattern = $"%MocId={mocId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Moc_Approve's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
