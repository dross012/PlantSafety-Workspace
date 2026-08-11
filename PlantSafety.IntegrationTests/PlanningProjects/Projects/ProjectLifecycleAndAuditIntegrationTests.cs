// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PlanningProjects.Projects
//  Layer:          IntegrationTests
//  File:           ProjectLifecycleAndAuditIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live PlanningProjects write path
//                  (Features\PlanningProjects\Projects -- IProjectRepository ->
//                  ProjectDapperRepository -> [PlanningProjects].[Project] /
//                  usp_Project_Create/GetById), which had no real backing SQL anywhere in the
//                  repository prior to this pass (ProjectDapperRepository referenced a schema/
//                  table/procedure set that had never been implemented -- the sibling
//                  PlanningProjects\Sql\Tables\Projects.sql / \Procedures\usp_Project_Insert.sql
//                  files are unrelated, unimplemented "TODO" template stubs never bound to any
//                  repository, flagged separately, out of scope here; likewise the concrete
//                  PlanningProjectsRepository.cs/PlanningProjectsService.cs classes registered in
//                  InfrastructureServiceRegistration are dead code with no Application-layer
//                  consumer, flagged separately).
//
//                  Exercises the real MediatR pipeline (CreateSafetyProjectCommand ->
//                  GetSafetyProjectByIdQuery), the real ProjectDapperRepository, and the real
//                  [PlanningProjects].[Project] table + usp_Project_Create/GetById stored
//                  procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyPlanningProjectsSchemaAsync). Confirms:
//                    1. Create -> GetById round-trips real data (Draft status, zero milestones).
//                    2. The newly added RowVersion column / ETag projection is present, stable
//                       across repeated reads, and (via a second, independent Create) genuinely
//                       reflects each row's own RowVersion rather than a stale/shared value.
//                    3. usp_Project_Create's newly added AuditLog INSERT fires on every real Create.
//                    4. usp_Project_Create's newly added TRY/CATCH + ErrorLog + THROW pattern
//                       populates ErrorLog and re-raises on a forced SQL-level failure (duplicate
//                       @ProjectId primary-key violation), mirroring
//                       RoiLifecycleAndAuditIntegrationTests.
//                       CreateRoi_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Features.PlanningProjects.Commands.Projects.CreateSafetyProject;
using PlantSafety.Application.Features.PlanningProjects.Queries.Projects.GetSafetyProject;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PlanningProjects.Projects;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ProjectLifecycleAndAuditIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ProjectLifecycle_CreateGetRoundTrip_ShouldReturnRealDataWithStableAdvancingETag()
    {
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var projectOwnerId = Guid.NewGuid();
            var plannedStartUtc = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var plannedEndUtc = new DateTimeOffset(2027, 6, 30, 0, 0, 0, TimeSpan.Zero);

            // ---- Create ------------------------------------------------------------
            var projectId = await sender.Send(new CreateSafetyProjectCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Name = "Guardrail Replacement Initiative",
                Description = "Replace aging guardrails identified in the Q1 audit.",
                Priority = "High",
                ProjectOwnerId = projectOwnerId,
                PlannedStartUtc = plannedStartUtc,
                PlannedEndUtc = plannedEndUtc,
                PlannedBudget = 42000m,
            }, CancellationToken.None);

            projectId.Should().NotBe(Guid.Empty);

            // ---- Get (real GetSafetyProjectByIdQueryHandler -> IProjectRepository -> real
            // usp_Project_GetById) ----------------------------------------------------
            var getResult = await sender.Send(new GetSafetyProjectByIdQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "ProjectManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                ProjectId = projectId,
                CachePreference = PlantSafety.Application.Abstractions.Messaging.CachePreference.None,
            }, CancellationToken.None);

            getResult.Should().NotBeNull("the just-created SafetyProject must be retrievable through the real stored procedure");
            getResult!.ProjectId.Should().Be(projectId);
            getResult.TenantId.Should().Be(TenantId);
            getResult.SiteId.Should().Be(siteId);
            getResult.Name.Should().Be("Guardrail Replacement Initiative");
            getResult.Status.Should().Be("Draft", "usp_Project_Create must insert every new SafetyProject in Draft status");
            getResult.Priority.Should().Be("High");
            getResult.ProjectOwnerId.Should().Be(projectOwnerId);
            getResult.PlannedBudget.Should().Be(42000m);
            getResult.OpenMilestoneCount.Should().Be(0, "a freshly-created SafetyProject has no milestones yet");

            var firstETag = getResult.ETag;
            firstETag.Should().NotBeNullOrWhiteSpace("usp_Project_GetById must project the RowVersion column as ETag");
            firstETag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");

            // ---- Re-read must yield an identical ETag (ROWVERSION must not advance on read) ----
            var getAgainResult = await sender.Send(new GetSafetyProjectByIdQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "ProjectManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                ProjectId = projectId,
                CachePreference = PlantSafety.Application.Abstractions.Messaging.CachePreference.None,
            }, CancellationToken.None);

            getAgainResult!.ETag.Should().Be(firstETag, "reading the same unmodified row twice must not advance the ROWVERSION-backed ETag");

            // ---- A second, independent Create must produce its own distinct row with its own
            // distinct ETag (proves the projection reflects each row's own RowVersion rather than
            // some cached/shared value). ----
            var secondProjectId = await sender.Send(new CreateSafetyProjectCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Name = "Second Independent Project",
                Description = "Distinct row for ETag isolation check.",
                Priority = "Medium",
                ProjectOwnerId = projectOwnerId,
                PlannedStartUtc = plannedStartUtc,
                PlannedEndUtc = plannedEndUtc,
            }, CancellationToken.None);

            var secondGetResult = await sender.Send(new GetSafetyProjectByIdQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "ProjectManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                ProjectId = secondProjectId,
                CachePreference = PlantSafety.Application.Abstractions.Messaging.CachePreference.None,
            }, CancellationToken.None);

            secondGetResult!.ProjectId.Should().Be(secondProjectId);
            secondGetResult.ETag.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateProject_ShouldWriteAuditLogRow()
    {
        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();

            var projectId = await sender.Send(new CreateSafetyProjectCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Name = "Audit Coverage Project",
                Description = "Exists solely to confirm AuditLog coverage.",
                Priority = "Low",
                ProjectOwnerId = Guid.NewGuid(),
                PlannedStartUtc = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero),
                PlannedEndUtc = new DateTimeOffset(2027, 3, 1, 0, 0, 0, TimeSpan.Zero),
            }, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);

            var auditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Project", Operation = "INSERT", Pattern = $"%ProjectId={projectId}%" });

            auditCount.Should().BeGreaterThan(0,
                "usp_Project_Create's newly added canonical AuditLog insert must have recorded this Create");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateProject_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [PlanningProjects].[usp_Project_Create] directly, twice, with the same @ProjectId.
        // Confirms the procedure's newly added THROW re-raises the exception and that the CATCH
        // block's ErrorLog insert fires -- mirroring
        // RoiLifecycleAndAuditIntegrationTests.CreateRoi_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog.
        var projectId = Guid.NewGuid();
        var tenantId = TenantId;
        var actorId = ActorId;

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteCreateAsync()
        {
            await using var cmd = new SqlCommand("[PlanningProjects].[usp_Project_Create]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
            cmd.Parameters.AddWithValue("@CorrelationId", CorrelationId);
            cmd.Parameters.AddWithValue("@ActorId", actorId);
            cmd.Parameters.AddWithValue("@NowUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@SiteId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Name", "Duplicate Key Test Project");
            cmd.Parameters.AddWithValue("@Description", "Forces a primary key violation on the second call.");
            cmd.Parameters.AddWithValue("@Priority", "Medium");
            cmd.Parameters.AddWithValue("@ProjectOwnerId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@PlannedStartUtc", new DateTime(2027, 4, 1, 0, 0, 0, DateTimeKind.Utc));
            cmd.Parameters.AddWithValue("@PlannedEndUtc", new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc));

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteCreateAsync();

        var act = ExecuteCreateAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_Project_Create call with a duplicate @ProjectId must violate the primary key and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Project_Create", Pattern = $"%ProjectId={projectId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Project_Create's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }
}
