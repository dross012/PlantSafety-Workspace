// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportIncidents
//  Layer:          Tests (Unit)
//  File:           GetTransportIncidentHistoryQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetTransportIncidentHistoryQueryHandler. Read-model
//                  repository is mocked with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Models;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportIncidents;

public sealed class GetTransportIncidentHistoryQueryHandlerTests : UnitTestBase
{
    private readonly ITransportIncidentRepository _repo = Substitute.For<ITransportIncidentRepository>();

    private static TransportIncidentItemDto Item() => new()
    {
        TransportIncidentId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        VehicleId = Guid.NewGuid(),
        DriverUserId = Guid.NewGuid(),
        IncidentType = "Collision",
        Severity = "Moderate",
        IncidentStatus = "Reported",
        Location = "Loading dock 4",
        OccurredAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingItems_ShouldReturnSuccessWithItems()
    {
        _repo.GetTransportIncidentHistoryAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<TransportIncidentItemDto> { Item() });
        var sut = new GetTransportIncidentHistoryQueryHandler(_repo, NullLogger<GetTransportIncidentHistoryQueryHandler>());

        var result = await sut.Handle(new GetTransportIncidentHistoryQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyItems()
    {
        _repo.GetTransportIncidentHistoryAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<TransportIncidentItemDto>());
        var sut = new GetTransportIncidentHistoryQueryHandler(_repo, NullLogger<GetTransportIncidentHistoryQueryHandler>());

        var result = await sut.Handle(new GetTransportIncidentHistoryQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.GetTransportIncidentHistoryAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<TransportIncidentItemDto>>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new GetTransportIncidentHistoryQueryHandler(_repo, NullLogger<GetTransportIncidentHistoryQueryHandler>());

        var result = await sut.Handle(new GetTransportIncidentHistoryQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.RetrievalFailed");
    }
}
