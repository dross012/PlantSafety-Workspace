// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.DriverQualifications
//  Layer:          Tests (Unit)
//  File:           DriverQualificationCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all DriverQualification command handlers: Qualify, Suspend,
//                  Reinstate, Retire. Repository is mocked with NSubstitute; no I/O, no network,
//                  <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification;

namespace PlantSafety.Tests.LogisticsTransportSafety.DriverQualifications;

public sealed class DriverQualificationCommandHandlerTests : UnitTestBase
{
    private static readonly Guid DriverUserId = new("0000000c-0000-0000-0000-00000000000c");
    private readonly IDriverQualificationRepository _repo = Substitute.For<IDriverQualificationRepository>();

    private static Agg Qualified() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, driverUserId: DriverUserId, licenseNumber: "CDL-1",
            issuedAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(-1)), expiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)));

    // ---- QualifyDriverCommandHandler ---------------------------------------------

    [Fact]
    public async Task Qualify_WithValidCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new QualifyDriverCommandHandler(_repo, NullLogger<QualifyDriverCommandHandler>());

        var result = await sut.Handle(new QualifyDriverCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            DriverUserId, "CommercialDriverLicense", "CDL-1", FixedUtcNow.AddYears(-1), FixedUtcNow.AddYears(1),
            TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Qualified");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Qualify_WithInvalidQualificationType_ShouldReturnFailure()
    {
        var sut = new QualifyDriverCommandHandler(_repo, NullLogger<QualifyDriverCommandHandler>());

        var result = await sut.Handle(new QualifyDriverCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            DriverUserId, "NotARealType", "CDL-1", FixedUtcNow.AddYears(-1), FixedUtcNow.AddYears(1),
            TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.InvalidQualificationType");
    }

    // ---- SuspendDriverQualificationCommandHandler ---------------------------------

    [Fact]
    public async Task Suspend_WhenAggregateExists_ShouldReturnSuccess()
    {
        var dq = Qualified();
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns(dq);
        var sut = new SuspendDriverQualificationCommandHandler(_repo, NullLogger<SuspendDriverQualificationCommandHandler>());

        var result = await sut.Handle(new SuspendDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            dq.DriverQualificationId.Value, "Failed drug test", TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Suspended");
        await _repo.Received(1).UpdateAsync(dq, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Suspend_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new SuspendDriverQualificationCommandHandler(_repo, NullLogger<SuspendDriverQualificationCommandHandler>());

        var result = await sut.Handle(new SuspendDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "reason", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.NotFound");
    }

    // ---- ReinstateDriverQualificationCommandHandler --------------------------------

    [Fact]
    public async Task Reinstate_WhenSuspended_ShouldReturnSuccess()
    {
        var dq = Qualified();
        dq.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns(dq);
        var sut = new ReinstateDriverQualificationCommandHandler(_repo, NullLogger<ReinstateDriverQualificationCommandHandler>());

        var result = await sut.Handle(new ReinstateDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            dq.DriverQualificationId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Qualified");
    }

    [Fact]
    public async Task Reinstate_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new ReinstateDriverQualificationCommandHandler(_repo, NullLogger<ReinstateDriverQualificationCommandHandler>());

        var result = await sut.Handle(new ReinstateDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.NotFound");
    }

    [Fact]
    public async Task Reinstate_WhenNotSuspended_ShouldReturnGenericFailure()
    {
        var dq = Qualified(); // never suspended
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns(dq);
        var sut = new ReinstateDriverQualificationCommandHandler(_repo, NullLogger<ReinstateDriverQualificationCommandHandler>());

        var result = await sut.Handle(new ReinstateDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            dq.DriverQualificationId.Value, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.ReinstateFailed");
    }

    // ---- RetireDriverQualificationCommandHandler -----------------------------------

    [Fact]
    public async Task Retire_WhenAggregateExists_ShouldReturnSuccess()
    {
        var dq = Qualified();
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns(dq);
        var sut = new RetireDriverQualificationCommandHandler(_repo, NullLogger<RetireDriverQualificationCommandHandler>());

        var result = await sut.Handle(new RetireDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            dq.DriverQualificationId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Retired");
    }

    [Fact]
    public async Task Retire_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DriverQualificationId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RetireDriverQualificationCommandHandler(_repo, NullLogger<RetireDriverQualificationCommandHandler>());

        var result = await sut.Handle(new RetireDriverQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.NotFound");
    }
}
