// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PlanningProjects
//  Layer:          IntegrationTests
//  File:           PlanningProjectsGetProjectsIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only PlanningProjects
//                  path (IPlanningProjectsRepository -> PlanningProjectsDapperRepository ->
//                  [PlanningProjects].[PlanningProjects] / usp_PlanningProjects_GetProjects),
//                  consumed by GetPlanningProjectsQueryHandler. This path has no write procedure,
//                  so ROWVERSION/ETag modernization does not apply here -- matches the Financials
//                  read-only [Financials].[Financials]/usp_Financials_GetRecords precedent (see
//                  ApplyFinancialsSchemaAsync's remarks). Seeds real rows directly into
//                  [PlanningProjects].[PlanningProjects] (applied against the Testcontainers SQL
//                  Server instance by TestDatabaseFactory.ApplyPlanningProjectsSchemaAsync) and
//                  exercises the real MediatR pipeline (GetPlanningProjectsQuery) end to end
//                  through the real DI container, confirming:
//                    1. Seeded rows within the requested [From, To] window and matching
//                       TenantId/SiteId are returned with correct field values.
//                    2. Soft-deleted rows (IsDeleted = 1) are excluded.
//                    3. Rows outside the requested date window are excluded.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.PlanningProjects.PlanningProjects.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PlanningProjects.Sql;

[Collection(nameof(IntegrationTestCollection))]
public sealed class PlanningProjectsGetProjectsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPlanningProjects_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var inRangeProjectId = Guid.NewGuid();
        var deletedProjectId = Guid.NewGuid();
        var outOfRangeProjectId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            // In-range, non-deleted -- must be returned.
            await conn.ExecuteAsync(
                @"INSERT INTO [PlanningProjects].[PlanningProjects]
                    (ProjectId, TenantId, SiteId, Name, Status, TargetCompletionUtc, OwnerId, IsDeleted)
                  VALUES (@ProjectId, @TenantId, @SiteId, @Name, @Status, @TargetCompletionUtc, @OwnerId, 0)",
                new
                {
                    ProjectId = inRangeProjectId,
                    TenantId,
                    SiteId = siteId,
                    Name = "Real Seeded Planning Project",
                    Status = "Planning",
                    TargetCompletionUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    OwnerId = ownerId,
                });

            // In-range, but soft-deleted -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [PlanningProjects].[PlanningProjects]
                    (ProjectId, TenantId, SiteId, Name, Status, TargetCompletionUtc, OwnerId, IsDeleted)
                  VALUES (@ProjectId, @TenantId, @SiteId, @Name, @Status, @TargetCompletionUtc, @OwnerId, 1)",
                new
                {
                    ProjectId = deletedProjectId,
                    TenantId,
                    SiteId = siteId,
                    Name = "Deleted Planning Project",
                    Status = "Planning",
                    TargetCompletionUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    OwnerId = ownerId,
                });

            // Outside the requested [fromUtc, toUtc] window -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [PlanningProjects].[PlanningProjects]
                    (ProjectId, TenantId, SiteId, Name, Status, TargetCompletionUtc, OwnerId, IsDeleted)
                  VALUES (@ProjectId, @TenantId, @SiteId, @Name, @Status, @TargetCompletionUtc, @OwnerId, 0)",
                new
                {
                    ProjectId = outOfRangeProjectId,
                    TenantId,
                    SiteId = siteId,
                    Name = "Out Of Range Planning Project",
                    Status = "Planning",
                    TargetCompletionUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    OwnerId = ownerId,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(
                new GetPlanningProjectsQuery(TenantId, siteId, fromUtc, toUtc),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetPlanningProjectsQueryHandler must succeed against the real stored procedure");
            result.Value!.TenantId.Should().Be(TenantId);
            result.Value.SiteId.Should().Be(siteId);

            result.Value.Items.Should().Contain(i => i.ProjectId == inRangeProjectId,
                "the in-range, non-deleted seeded row must be returned by the real usp_PlanningProjects_GetProjects");
            result.Value.Items.Should().NotContain(i => i.ProjectId == deletedProjectId,
                "usp_PlanningProjects_GetProjects filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.ProjectId == outOfRangeProjectId,
                "usp_PlanningProjects_GetProjects filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.ProjectId == inRangeProjectId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.Name.Should().Be("Real Seeded Planning Project");
            item.Status.Should().Be("Planning");
            item.OwnerId.Should().Be(ownerId);
            item.TargetCompletionUtc.Should().Be(new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        }
        finally
        {
            scope.Dispose();
        }
    }
}
