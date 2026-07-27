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
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
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

            ((object)row).Should().NotBeNull("CreateMocCommand must persist a real row via usp_Moc_Create");
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
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
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
}
