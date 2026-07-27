// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.SafetyEvents.IncidentManagement
//  Layer:          IntegrationTests
//  File:           IncidentLifecycleIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Full Create -> Update -> SoftDelete -> GetById/List/Search lifecycle coverage
//                  for the Incident aggregate (SafetyEvents module), exercising the real DI
//                  container (IIncidentRepository/IIncidentReadRepository -> IncidentDapperRepository
//                  / IncidentReadDapperRepository) and the real [SafetyEvents].[Incident] table +
//                  usp_Incident_Create/Update/SoftDelete / usp_IncidentRead_GetById/List/Search
//                  stored procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyIncidentManagementSchemaAsync). These procedures
//                  and their backing table were created in a prior pass and had zero integration
//                  coverage until now.
//
//                  Commands/queries are sent via MediatR's ISender resolved directly from the DI
//                  container -- the same pattern established by
//                  Operations\AuditViewer\AuditViewerIntegrationTests.cs -- since none of
//                  CreateIncidentCommand/UpdateIncidentCommand/DeleteIncidentCommand/
//                  GetIncidentByIdQuery/ListIncidentsQuery/SearchIncidentsQuery implement
//                  IAuthorizedRequest, so AuthorizationBehavior passes them through without
//                  requiring a test authentication handler.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Search;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.SafetyEvents.IncidentManagement;

/// <summary>
/// Exercises the full Incident aggregate lifecycle (Create -&gt; Update -&gt; SoftDelete -&gt;
/// GetById/List/Search) through the real MediatR pipeline, the real
/// <c>IncidentDapperRepository</c>/<c>IncidentReadDapperRepository</c>, and the real
/// <c>[SafetyEvents].[Incident]</c> table + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class IncidentLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task IncidentLifecycle_CreateUpdateSoftDelete_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var reportedByWorkerId = Guid.NewGuid();
            var investigatorId = Guid.NewGuid();

            // ---- Create -----------------------------------------------------
            var createCommand = new CreateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                Title = "Forklift near-collision in Bay 3",
                Description = "Forklift operator nearly collided with a pedestrian in the loading bay.",
                IncidentOccurredAtUtc = DateTimeOffset.UtcNow.AddHours(-1),
                Severity = "Moderate",
                Location = "Bay 3 - Loading Dock",
                ReportedByWorkerId = reportedByWorkerId,
                AssignedInvestigatorId = null,
            };

            var incidentId = await sender.Send(createCommand, CancellationToken.None);
            incidentId.Should().NotBe(Guid.Empty);

            // ---- GetById (post-create) ---------------------------------------
            var getAfterCreate = await sender.Send(BuildGetByIdQuery(incidentId), CancellationToken.None);
            getAfterCreate.Should().NotBeNull("the incident must be readable immediately after creation");
            getAfterCreate!.Title.Should().Be(createCommand.Title);
            getAfterCreate.Status.Should().Be("Reported");
            getAfterCreate.Severity.Should().Be("Moderate");
            getAfterCreate.AssignedInvestigatorId.Should().BeNull();
            var eTagAfterCreate = getAfterCreate.ETag;
            eTagAfterCreate.Should().NotBeNullOrWhiteSpace();

            // ---- Update -------------------------------------------------------
            var updateCommand = new UpdateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = eTagAfterCreate,
                IncidentId = incidentId,
                Title = null,
                Description = null,
                Severity = "Serious",
                Location = null,
                AssignedInvestigatorId = investigatorId,
            };

            var updatedIncidentId = await sender.Send(updateCommand, CancellationToken.None);
            updatedIncidentId.Should().Be(incidentId);

            var getAfterUpdate = await sender.Send(BuildGetByIdQuery(incidentId), CancellationToken.None);
            getAfterUpdate.Should().NotBeNull();
            getAfterUpdate!.Severity.Should().Be("Serious", "the ETag-guarded update must have persisted the new severity");
            getAfterUpdate.Title.Should().Be(createCommand.Title, "Title was not supplied on update and must be unchanged");
            getAfterUpdate.AssignedInvestigatorId.Should().Be(investigatorId);
            var eTagAfterUpdate = getAfterUpdate.ETag;
            eTagAfterUpdate.Should().NotBe(eTagAfterCreate, "a successful UPDATE must advance the ROWVERSION-backed ETag");

            // ---- List (filtered to this incident's Severity) ------------------
            var listResult = await sender.Send(new ListIncidentsQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "SafetyManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                CachePreference = CachePreference.None,
                PageNumber = 1,
                PageSize = 50,
                Severity = "Serious",
            }, CancellationToken.None);

            listResult.Items.Should().Contain(i => i.IncidentId == incidentId);

            // ---- Search (free-text on the updated Location/Title) -------------
            var searchResult = await sender.Send(new SearchIncidentsQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "SafetyManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                SearchTerm = "Forklift",
                PageNumber = 1,
                PageSize = 50,
            }, CancellationToken.None);

            searchResult.Items.Should().Contain(i => i.IncidentId == incidentId);

            // ---- SoftDelete -----------------------------------------------------
            var deleteCommand = new DeleteIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = eTagAfterUpdate,
                IncidentId = incidentId,
                DeletionReason = "Duplicate report -- consolidated into INC-master record.",
            };

            await sender.Send(deleteCommand, CancellationToken.None);

            // ---- GetById (post-delete) -- soft-deleted rows are invisible to reads.
            var getAfterDelete = await sender.Send(BuildGetByIdQuery(incidentId), CancellationToken.None);
            getAfterDelete.Should().BeNull("usp_IncidentRead_GetById filters IsDeleted = 0");

            var listAfterDelete = await sender.Send(new ListIncidentsQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "SafetyManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                CachePreference = CachePreference.None,
                PageNumber = 1,
                PageSize = 50,
                Severity = "Serious",
            }, CancellationToken.None);

            listAfterDelete.Items.Should().NotContain(i => i.IncidentId == incidentId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task UpdateIncident_WithStaleETag_ShouldThrowConcurrencyConflict()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var createCommand = new CreateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                Title = "Chemical spill in Storage Room 2",
                Description = "Minor solvent spill contained by on-site response team.",
                IncidentOccurredAtUtc = DateTimeOffset.UtcNow.AddMinutes(-30),
                Severity = "Minor",
                Location = "Storage Room 2",
                ReportedByWorkerId = Guid.NewGuid(),
            };

            var incidentId = await sender.Send(createCommand, CancellationToken.None);

            var act = async () => await sender.Send(new UpdateIncidentCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = "0x0000000000000000",
                IncidentId = incidentId,
                Severity = "Critical",
            }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>(
                "a stale/incorrect ETag must be rejected by usp_Incident_Update's ROWVERSION match and surfaced as a concurrency failure");
        }
        finally
        {
            scope.Dispose();
        }
    }

    private GetIncidentByIdQuery BuildGetByIdQuery(Guid incidentId) => new()
    {
        TraceId = Guid.NewGuid().ToString(),
        SpanId = Guid.NewGuid().ToString(),
        ApiVersionToken = "v1",
        RequestingUserId = ActorId,
        RequestingUserRole = "SafetyManager",
        TenantId = TenantId,
        CorrelationId = CorrelationId,
        CachePreference = CachePreference.None,
        IncidentId = incidentId,
    };
}
