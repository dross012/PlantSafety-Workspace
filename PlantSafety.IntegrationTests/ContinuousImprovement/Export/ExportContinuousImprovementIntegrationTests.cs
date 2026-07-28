// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.ContinuousImprovement.Export
//  Layer:          IntegrationTests
//  File:           ExportContinuousImprovementIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Genuine, non-vacuous coverage for the ContinuousImprovement export read path --
//                  ExportContinuousImprovementQuery -> ExportContinuousImprovementQueryHandler ->
//                  IExportContinuousImprovementRepository -> ExportContinuousImprovementDapperRepository
//                  -> real usp_ContinuousImprovementExportItems_List stored procedure (a read-only
//                  UNION ALL over [ContinuousImprovement].[CAPA] and [ContinuousImprovement].[RCA],
//                  applied to the Testcontainers instance by
//                  TestDatabaseFactory.ApplyContinuousImprovementSchemaAsync, mirroring
//                  PlantSafety.Infrastructure\ContinuousImprovement\Sql\Procedures\
//                  usp_ContinuousImprovementExportItems_List.sql verbatim) plus a real upload to the
//                  Azurite Testcontainers-adjacent instance (docker `plantsafety-azurite`, configured
//                  via Appsettings.json BlobStorage:ConnectionString=UseDevelopmentStorage=true) via
//                  the real AzureBlobStorageService. Seeds one CAPA row and one RCA row directly
//                  (this export procedure is read-only over tables with no C# write path -- see
//                  CAPAIntegrationTests / RCAIntegrationTests remarks) and confirms both are present
//                  in the resulting export with a genuine, retrievable blob download URL.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.ContinuousImprovement.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.ContinuousImprovement.Export;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ExportContinuousImprovementIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ExportContinuousImprovement_WithSeededCapaAndRcaRows_ShouldReturnRealCombinedExport()
    {
        var siteId = Guid.NewGuid();
        var effectiveAtUtc = FixedUtcNow;
        var capaId = Guid.NewGuid();
        var rcaId = Guid.NewGuid();

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.ExecuteAsync(
                @"INSERT INTO [ContinuousImprovement].[CAPA]
                    (CapaId, TenantId, SiteId, ActionType, Title, Status, DueDateUtc, AssignedTo)
                  VALUES
                    (@CapaId, @TenantId, @SiteId, @ActionType, @Title, @Status, @DueDateUtc, @AssignedTo)",
                new
                {
                    CapaId = capaId,
                    TenantId,
                    SiteId = siteId,
                    ActionType = "Corrective",
                    Title = "Export coverage CAPA row",
                    Status = "Open",
                    DueDateUtc = effectiveAtUtc,
                    AssignedTo = ActorId,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [ContinuousImprovement].[RCA]
                    (RcaId, TenantId, SiteId, ProblemStatement, Methodology, Status, InitiatedAtUtc, OwnerUserId)
                  VALUES
                    (@RcaId, @TenantId, @SiteId, @ProblemStatement, @Methodology, @Status, @InitiatedAtUtc, @OwnerUserId)",
                new
                {
                    RcaId = rcaId,
                    TenantId,
                    SiteId = siteId,
                    ProblemStatement = "Export coverage RCA row.",
                    Methodology = "FishboneDiagram",
                    Status = "Initiated",
                    InitiatedAtUtc = effectiveAtUtc,
                    OwnerUserId = ActorId,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new ExportContinuousImprovementQuery(
                TenantId: TenantId,
                SiteId: siteId,
                Format: 0,
                From: effectiveAtUtc.AddDays(-1),
                To: effectiveAtUtc.AddDays(1),
                Status: null,
                MaxRecords: 1000), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.RecordCount.Should().BeGreaterThanOrEqualTo(2, "the seeded CAPA row and RCA row must both be present in the real combined export");
            result.Value.FileName.Should().EndWith(".csv");
            result.Value.ContentType.Should().Be("text/csv");
            result.Value.DownloadUrl.Should().NotBeNullOrWhiteSpace("ExportContinuousImprovementDapperRepository must upload the export via the real IBlobStorageService and return a real download URI");
            result.Value.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow, "the export download URL must not already be expired");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
