// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportIncidents
//  Layer:          Tests (Unit)
//  File:           TransportIncidentCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all TransportIncident command handlers: Record,
//                  BeginInvestigation, Close. Repository is mocked with NSubstitute; no I/O, no
//                  network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportIncidents;

public sealed class TransportIncidentCommandHandlerTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("00000011-0000-0000-0000-000000000011");
    private static readonly Guid DriverUserId = new("00000012-0000-0000-0000-000000000012");
    private readonly ITransportIncidentRepository _repo = Substitute.For<ITransportIncidentRepository>();

    private static Agg Reported() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, driverUserId: DriverUserId,
            location: "Loading dock 4");

    // ---- RecordTransportIncidentCommandHandler --------------------------------------

    [Fact]
    public async Task Record_WithValidCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new RecordTransportIncidentCommandHandler(_repo, NullLogger<RecordTransportIncidentCommandHandler>());

        var result = await sut.Handle(new RecordTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, DriverUserId, "Collision", "Moderate", "Loading dock 4", TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Reported");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_WithInvalidIncidentType_ShouldReturnFailure()
    {
        var sut = new RecordTransportIncidentCommandHandler(_repo, NullLogger<RecordTransportIncidentCommandHandler>());

        var result = await sut.Handle(new RecordTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, DriverUserId, "NotARealType", "Moderate", "Loading dock 4", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.InvalidIncidentType");
    }

    [Fact]
    public async Task Record_WithInvalidSeverity_ShouldReturnFailure()
    {
        var sut = new RecordTransportIncidentCommandHandler(_repo, NullLogger<RecordTransportIncidentCommandHandler>());

        var result = await sut.Handle(new RecordTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, DriverUserId, "Collision", "NotARealSeverity", "Loading dock 4", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.InvalidSeverity");
    }

    // ---- BeginInvestigationTransportIncidentCommandHandler --------------------------

    [Fact]
    public async Task BeginInvestigation_WhenAggregateExists_ShouldReturnSuccess()
    {
        var incident = Reported();
        _repo.GetByIdAsync(Arg.Any<TransportIncidentId>(), Arg.Any<CancellationToken>()).Returns(incident);
        var sut = new BeginInvestigationTransportIncidentCommandHandler(_repo, NullLogger<BeginInvestigationTransportIncidentCommandHandler>());

        var result = await sut.Handle(new BeginInvestigationTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            incident.TransportIncidentId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("UnderInvestigation");
        await _repo.Received(1).UpdateAsync(incident, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BeginInvestigation_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportIncidentId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new BeginInvestigationTransportIncidentCommandHandler(_repo, NullLogger<BeginInvestigationTransportIncidentCommandHandler>());

        var result = await sut.Handle(new BeginInvestigationTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.NotFound");
    }

    // ---- CloseTransportIncidentCommandHandler ----------------------------------------

    [Fact]
    public async Task Close_WhenUnderInvestigation_ShouldReturnSuccess()
    {
        var incident = Reported();
        incident.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<TransportIncidentId>(), Arg.Any<CancellationToken>()).Returns(incident);
        var sut = new CloseTransportIncidentCommandHandler(_repo, NullLogger<CloseTransportIncidentCommandHandler>());

        var result = await sut.Handle(new CloseTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            incident.TransportIncidentId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Close_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportIncidentId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new CloseTransportIncidentCommandHandler(_repo, NullLogger<CloseTransportIncidentCommandHandler>());

        var result = await sut.Handle(new CloseTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.NotFound");
    }

    [Fact]
    public async Task Close_WhenReported_ShouldReturnGenericFailure()
    {
        var incident = Reported(); // never began investigation
        _repo.GetByIdAsync(Arg.Any<TransportIncidentId>(), Arg.Any<CancellationToken>()).Returns(incident);
        var sut = new CloseTransportIncidentCommandHandler(_repo, NullLogger<CloseTransportIncidentCommandHandler>());

        var result = await sut.Handle(new CloseTransportIncidentCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            incident.TransportIncidentId.Value, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.CloseFailed");
    }
}
