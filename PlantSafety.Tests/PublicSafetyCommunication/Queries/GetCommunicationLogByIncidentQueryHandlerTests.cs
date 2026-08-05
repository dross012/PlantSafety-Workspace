// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Queries
//  Layer:          Tests (Unit)
//  File:           GetCommunicationLogByIncidentQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for GetCommunicationLogByIncidentQueryHandler. Isolated,
//                  deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Handlers;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Models;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Repositories;

namespace PlantSafety.Tests.PublicSafetyCommunication.Queries;

public sealed class GetCommunicationLogByIncidentQueryHandlerTests : UnitTestBase
{
    private readonly ICommunicationLogEntryRepository _repo = Substitute.For<ICommunicationLogEntryRepository>();

    private GetCommunicationLogByIncidentQueryHandler Sut() => new(_repo, NullLogger<GetCommunicationLogByIncidentQueryHandler>());

    private static CommunicationLogEntryItemDto SampleItem(Guid tenantId, Guid incidentId) => new()
    {
        CommunicationLogEntryId = Guid.NewGuid(),
        TenantId = tenantId,
        IncidentId = incidentId,
        Direction = "Outbound",
        Medium = "Phone",
        CounterpartyName = "County Fire Dispatch",
        Summary = "Notified county fire dispatch of the release.",
        OccurredAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
    };

    [Fact]
    public async Task Handle_ReturnsItemsFromRepositoryScopedToIncident()
    {
        var incidentId = Guid.NewGuid();
        var items = new[] { SampleItem(TestTenantId.Value, incidentId) };
        _repo.GetCommunicationLogByIncidentAsync(TestTenantId.Value, incidentId, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new GetCommunicationLogByIncidentQuery(TestTenantId.Value, incidentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IncidentId.Should().Be(incidentId);
        result.Value.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.GetCommunicationLogByIncidentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CommunicationLogEntryItemDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(new GetCommunicationLogByIncidentQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.CommunicationLogEntry.RetrievalFailed");
    }
}
