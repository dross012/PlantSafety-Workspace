// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Compliance.Permits
//  Layer:          IntegrationTests
//  File:           PermitLifecycleIntegrationTests.cs
//  Created:        2026-07-27
//  SchemaVersion:  2026.1
//  Description:    Full Create -> GetById -> Approve -> List lifecycle coverage for the Permit
//                  (Permit to Work) aggregate -- CreatePermitCommand and ApprovePermitCommand are
//                  two of only three real Compliance write actions (see
//                  docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md
//                  Sec 1.2). Exercises the real MediatR pipeline, the real
//                  PermitDapperRepository, and the real [Compliance].[Permit] table +
//                  usp_Permit_Create/Approve/GetById/List stored procedures applied to the
//                  Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyComplianceSchemaAsync). Neither the Permit
//                  table nor its stored procedures existed anywhere in the codebase before this
//                  pass, even though PermitDapperRepository referenced them by name -- neither
//                  CreatePermitCommand nor ApprovePermitCommand could ever have succeeded against
//                  a real database until now.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Create;
using PlantSafety.Application.Features.Compliance.Queries.Permits.Get;
using PlantSafety.Application.Features.Compliance.Queries.Permits.List;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Compliance.Permits;

/// <summary>
/// Exercises the Permit-to-Work Create -&gt; GetById -&gt; Approve -&gt; List lifecycle through the
/// real MediatR pipeline, the real <c>PermitDapperRepository</c>, and the real
/// <c>[Compliance].[Permit]</c> table + stored procedures.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class PermitLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PermitLifecycle_CreateGetApprove_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var workLeadId = Guid.NewGuid();

            // ---- Create -----------------------------------------------------
            var createCommand = new CreatePermitCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                PermitType = "HotWork",
                WorkDescription = "Welding repair on catwalk handrail bracket.",
                WorkLocation = "Unit 3 - Catwalk Level 2",
                WorkLeadId = workLeadId,
                ValidFromUtc = DateTimeOffset.UtcNow.AddHours(1),
                ValidToUtc = DateTimeOffset.UtcNow.AddHours(9),
            };

            var permitId = await sender.Send(createCommand, CancellationToken.None);
            permitId.Should().NotBe(Guid.Empty);

            // ---- GetById (post-create) ---------------------------------------
            var getAfterCreate = await sender.Send(BuildGetByIdQuery(permitId), CancellationToken.None);
            getAfterCreate.Should().NotBeNull("the permit must be readable immediately after creation");
            getAfterCreate!.PermitType.Should().Be("HotWork");
            getAfterCreate.Status.Should().Be("Draft");
            getAfterCreate.PermitNumber.Should().NotBeNullOrWhiteSpace("usp_Permit_Create must generate a human-readable PermitNumber");
            getAfterCreate.ApprovedByActorId.Should().BeNull();
            var eTagAfterCreate = getAfterCreate.ETag;
            eTagAfterCreate.Should().NotBeNullOrWhiteSpace();

            // ---- Approve --------------------------------------------------------
            var approveCommand = new ApprovePermitCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = eTagAfterCreate,
                PermitId = permitId,
                ApprovalNotes = "Fire watch confirmed, extinguisher on site.",
            };

            var approvedPermitId = await sender.Send(approveCommand, CancellationToken.None);
            approvedPermitId.Should().Be(permitId);

            var getAfterApprove = await sender.Send(BuildGetByIdQuery(permitId), CancellationToken.None);
            getAfterApprove.Should().NotBeNull();
            getAfterApprove!.Status.Should().Be("Active", "a successful approval must transition the permit to Active");
            getAfterApprove.ApprovedByActorId.Should().Be(ActorId);
            getAfterApprove.ApprovedAtUtc.Should().NotBeNull();
            getAfterApprove.ETag.Should().NotBe(eTagAfterCreate, "a successful approval must advance the ROWVERSION-backed ETag");

            // ---- List (filtered to this permit's type) -------------------------
            var listResult = await sender.Send(new ListPermitsQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "SafetyManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                PageNumber = 1,
                PageSize = 50,
                PermitType = "HotWork",
            }, CancellationToken.None);

            listResult.Items.Should().Contain(p => p.PermitId == permitId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ApprovePermit_WithStaleETag_ShouldThrowConcurrencyConflict()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var permitId = await sender.Send(new CreatePermitCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = Guid.NewGuid(),
                PermitType = "ConfinedSpace",
                WorkDescription = "Tank interior inspection -- attendant required.",
                WorkLocation = "Tank Farm - Tank 12",
                WorkLeadId = Guid.NewGuid(),
                ValidFromUtc = DateTimeOffset.UtcNow.AddHours(2),
                ValidToUtc = DateTimeOffset.UtcNow.AddHours(6),
            }, CancellationToken.None);

            var act = async () => await sender.Send(new ApprovePermitCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = null,
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                ETag = "0x0000000000000000",
                PermitId = permitId,
                ApprovalNotes = "Should be rejected -- stale ETag.",
            }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>(
                "a stale/incorrect ETag must be rejected by usp_Permit_Approve's ROWVERSION match and surfaced as a concurrency failure");
        }
        finally
        {
            scope.Dispose();
        }
    }

    private GetPermitByIdQuery BuildGetByIdQuery(Guid permitId) => new()
    {
        TraceId = Guid.NewGuid().ToString(),
        SpanId = Guid.NewGuid().ToString(),
        ApiVersionToken = "v1",
        RequestingUserId = ActorId,
        RequestingUserRole = "SafetyManager",
        TenantId = TenantId,
        CorrelationId = CorrelationId,
        CachePreference = CachePreference.None,
        PermitId = permitId,
    };
}
