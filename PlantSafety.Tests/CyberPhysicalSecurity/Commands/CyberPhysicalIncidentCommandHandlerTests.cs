// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Commands
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalIncidentCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for OpenCyberPhysicalIncidentCommandHandler and
//                  CloseCyberPhysicalIncidentCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;
using CyberPhysicalIncidentAggregate = PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates.CyberPhysicalIncident;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Commands;

public sealed class OpenCyberPhysicalIncidentCommandHandlerTests : UnitTestBase
{
    private readonly ICyberPhysicalIncidentRepository _repo = Substitute.For<ICyberPhysicalIncidentRepository>();

    private OpenCyberPhysicalIncidentCommandHandler Sut() => new(_repo, NullLogger<OpenCyberPhysicalIncidentCommandHandler>());

    private static OpenCyberPhysicalIncidentCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        "Coordinated PLC tampering", "description", "High", TestUserId.Value, null, TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Open");
        await _repo.Received(1).AddAsync(Arg.Any<CyberPhysicalIncidentAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedSeverity_DefaultsToMedium()
    {
        CyberPhysicalIncidentAggregate? captured = null;
        await _repo.AddAsync(Arg.Do<CyberPhysicalIncidentAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        var result = await Sut().Handle(ValidCommand() with { Severity = "NotARealSeverity" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.Severity.ToString().Should().Be("Medium");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.AddAsync(Arg.Any<CyberPhysicalIncidentAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.CyberPhysicalIncident.OpenFailed");
    }
}

public sealed class CloseCyberPhysicalIncidentCommandHandlerTests : UnitTestBase
{
    private readonly ICyberPhysicalIncidentRepository _repo = Substitute.For<ICyberPhysicalIncidentRepository>();

    private CloseCyberPhysicalIncidentCommandHandler Sut() => new(_repo, NullLogger<CloseCyberPhysicalIncidentCommandHandler>());

    private static CyberPhysicalIncidentAggregate OpenedIncident() => CyberPhysicalIncidentAggregate.Open(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
        title: "Incident", description: "desc", severity: PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums.CyberPhysicalIncidentSeverity.High,
        ownerId: TestUserId);

    private static CloseCyberPhysicalIncidentCommand CommandFor(CyberPhysicalIncidentAggregate incident) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value,
        incident.CyberPhysicalIncidentId.Value, TestUserId.Value);

    [Fact]
    public async Task Handle_WithOpenIncident_ClosesDirectlyAndComputesResponseTime()
    {
        var incident = OpenedIncident();
        _repo.GetByIdAsync(incident.CyberPhysicalIncidentId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(incident);

        var result = await Sut().Handle(CommandFor(incident), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
        result.Value.ResponseTimeMinutes.Should().BeGreaterThanOrEqualTo(0);
        await _repo.Received(1).UpdateAsync(incident, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenIncidentDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CyberPhysicalIncidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CyberPhysicalIncidentAggregate?)null);

        var command = new CloseCyberPhysicalIncidentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.CyberPhysicalIncident.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAlreadyClosed_ReturnsDomainFailureNotUnhandledException()
    {
        var incident = OpenedIncident();
        incident.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(incident.CyberPhysicalIncidentId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(incident);

        var result = await Sut().Handle(CommandFor(incident), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.CyberPhysicalIncident.InvalidTransition");
        result.Error.Severity.Should().Be(ErrorSeverity.Domain);
    }
}
