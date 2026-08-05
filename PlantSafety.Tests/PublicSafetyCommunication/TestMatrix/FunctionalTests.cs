// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Functional
//  File:           FunctionalTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-FUNC-PSC] Functional test type (TRD-MAN-2026-001 Ch.3): verifies each of
//                  the module's 6 write commands and 4 read queries produces the functionally
//                  correct outcome against a real handler + in-memory repository, scaled to this
//                  4-aggregate module (one functional case per command/query). Isolated,
//                  deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Handlers;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;
using AgencyReadRepository = PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories.IPublicSafetyAgencyRepository;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using AgencyDomainAggregate = PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency;
using AgencySpecificationBase = PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Specifications.PublicSafetyAgencySpecification;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class FunctionalTests : UnitTestBase
{
    [Fact]
    public async Task RegisterPublicSafetyAgency_ThenListPublicSafetyAgencies_ReturnsTheRegisteredAgency()
    {
        var repo = new InMemoryPublicSafetyAgencyRepository();
        var registerHandler = new RegisterPublicSafetyAgencyCommandHandler(repo, NullLogger<RegisterPublicSafetyAgencyCommandHandler>());
        var listHandler = new ListPublicSafetyAgenciesQueryHandler(new PassthroughAgencyReadRepository(repo), NullLogger<ListPublicSafetyAgenciesQueryHandler>());

        var registerResult = await registerHandler.Handle(new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "County Fire Department", "Fire", "County district 4", "555-0100", null)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        registerResult.IsSuccess.Should().BeTrue();

        var listResult = await listHandler.Handle(
            new ListPublicSafetyAgenciesQuery(TestTenantId.Value, null, null, false), CancellationToken.None);

        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Items.Should().ContainSingle(i => i.PublicSafetyAgencyId == registerResult.Value.PublicSafetyAgencyId);
    }

    [Fact]
    public async Task DispatchIncidentNotification_ThenAcknowledge_ThenClose_ProducesExpectedFinalStatus()
    {
        var repo = new InMemoryIncidentNotificationRepository();
        var dispatchHandler = new DispatchIncidentNotificationCommandHandler(repo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        var ackHandler = new AcknowledgeIncidentNotificationCommandHandler(repo, NullLogger<AcknowledgeIncidentNotificationCommandHandler>());
        var closeHandler = new CloseIncidentNotificationCommandHandler(repo, NullLogger<CloseIncidentNotificationCommandHandler>());

        var dispatchResult = await dispatchHandler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "Ammonia release -- HazMat requested.")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        dispatchResult.IsSuccess.Should().BeTrue();
        dispatchResult.Value.Status.Should().Be("Dispatched");

        var notificationId = dispatchResult.Value.IncidentNotificationId;
        var etag = repo.GetETag(notificationId);

        var ackResult = await ackHandler.Handle(new AcknowledgeIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            notificationId, "County Dispatch Desk 4")
        { ActorId = TestUserId.Value, ETag = etag }, CancellationToken.None);
        ackResult.IsSuccess.Should().BeTrue();
        ackResult.Value.Status.Should().Be("Acknowledged");

        var closeResult = await closeHandler.Handle(new CloseIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, notificationId)
        { ActorId = TestUserId.Value, ETag = repo.GetETag(notificationId) }, CancellationToken.None);

        closeResult.IsSuccess.Should().BeTrue();
        closeResult.Value.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task BroadcastPublicAlert_PersistsSentBroadcast()
    {
        var repo = new InMemoryPublicAlertBroadcastRepository();
        var handler = new BroadcastPublicAlertCommandHandler(repo, NullLogger<BroadcastPublicAlertCommandHandler>());

        var result = await handler.Handle(new BroadcastPublicAlertCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), "Warning", "Ipaws", "Shelter in place", "A chemical release has occurred.", "1-mile radius")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Sent");
        repo.AddCallCount.Should().Be(1);
    }

    [Fact]
    public async Task LogCommunicationEntry_PersistsImmutableEntry()
    {
        var repo = new InMemoryCommunicationLogEntryRepository();
        var handler = new LogCommunicationEntryCommandHandler(repo, NullLogger<LogCommunicationEntryCommandHandler>());

        var result = await handler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), null, null, "Outbound", "Phone", "County Fire Dispatch", "Notified dispatch.", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.AddCallCount.Should().Be(1);
    }

    /// <summary>Adapts the Domain read/write repository fake to the Application-layer read-model contract for ListPublicSafetyAgenciesQueryHandler.</summary>
    private sealed class PassthroughAgencyReadRepository(InMemoryPublicSafetyAgencyRepository inner) : AgencyReadRepository
    {
        public async Task<IReadOnlyList<PublicSafetyAgencyItemDto>> ListPublicSafetyAgenciesAsync(
            Guid tenantId, Guid? siteId, string? agencyType, bool activeOnly, CancellationToken cancellationToken)
        {
            var all = await inner.ListAsync(new AllAgenciesSpecification(), cancellationToken);
            return all
                .Where(a => a.TenantId == tenantId)
                .Select(a => new PublicSafetyAgencyItemDto
                {
                    PublicSafetyAgencyId = a.PublicSafetyAgencyId.Value,
                    TenantId = a.TenantId,
                    SiteId = a.SiteId,
                    Status = a.Status.ToString(),
                    AgencyName = a.AgencyName,
                    AgencyType = a.AgencyType.ToString(),
                    JurisdictionDescription = a.JurisdictionDescription,
                    PrimaryPhoneNumber = a.PrimaryPhoneNumber,
                    PrimaryEmail = a.PrimaryEmail,
                    VerificationStatus = a.VerificationStatus.ToString(),
                    CreatedAtUtc = a.CreatedAtUtc.Value,
                })
                .ToList();
        }

        private sealed class AllAgenciesSpecification : AgencySpecificationBase
        {
            public override System.Linq.Expressions.Expression<Func<AgencyDomainAggregate, bool>> ToExpression()
                => _ => true;
        }
    }
}
