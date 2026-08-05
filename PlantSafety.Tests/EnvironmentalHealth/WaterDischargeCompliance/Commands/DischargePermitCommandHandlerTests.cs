// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           DischargePermitCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the DischargePermit aggregate: Issue, Renew,
//                  FlagExpiringSoon, Suspend, Revoke.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class DischargePermitCommandHandlerTests : UnitTestBase
{
    private readonly IDischargePermitRepository _repo = Substitute.For<IDischargePermitRepository>();

    private static Agg IssuedPermit() => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        permitNumber: "NPDES-2026-0001", regulatoryProgram: RegulatoryProgramType.Npdes,
        issuingAuthority: "State EPA", effectiveAtUtc: FixedTimestamp,
        expiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)));

    [Fact]
    public async Task Issue_WithValidCommand_PersistsNewPermitAndReturnsActiveStatus()
    {
        var sut = new IssueDischargePermitCommandHandler(_repo, NullLogger<IssueDischargePermitCommandHandler>());
        var command = new IssueDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            "NPDES-2026-0099", "Npdes", "State EPA", FixedUtcNow, FixedUtcNow.AddYears(1),
            "Standard conditions.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePermitStatus.Active));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Renew_WhenPermitExists_AdvancesExpiryAndPersists()
    {
        var permit = IssuedPermit();
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns(permit);
        var sut = new RenewDischargePermitCommandHandler(_repo, NullLogger<RenewDischargePermitCommandHandler>());
        var command = new RenewDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            permit.DischargePermitId.Value, FixedUtcNow.AddYears(2), TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePermitStatus.Active));
        await _repo.Received(1).UpdateAsync(permit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Renew_WhenPermitDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RenewDischargePermitCommandHandler(_repo, NullLogger<RenewDischargePermitCommandHandler>());
        var command = new RenewDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), FixedUtcNow.AddYears(1), TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }

    [Fact]
    public async Task FlagExpiringSoon_WhenActive_TransitionsToExpiringSoon()
    {
        var permit = IssuedPermit();
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns(permit);
        var sut = new FlagDischargePermitExpiringSoonCommandHandler(_repo, NullLogger<FlagDischargePermitExpiringSoonCommandHandler>());
        var command = new FlagDischargePermitExpiringSoonCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            permit.DischargePermitId.Value, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePermitStatus.ExpiringSoon));
    }

    [Fact]
    public async Task Suspend_WhenActive_TransitionsToSuspendedAndPersists()
    {
        var permit = IssuedPermit();
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns(permit);
        var sut = new SuspendDischargePermitCommandHandler(_repo, NullLogger<SuspendDischargePermitCommandHandler>());
        var command = new SuspendDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            permit.DischargePermitId.Value, "Effluent violation.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePermitStatus.Suspended));
        await _repo.Received(1).UpdateAsync(permit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_WhenActive_TransitionsToRevokedAndPersists()
    {
        var permit = IssuedPermit();
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns(permit);
        var sut = new RevokeDischargePermitCommandHandler(_repo, NullLogger<RevokeDischargePermitCommandHandler>());
        var command = new RevokeDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            permit.DischargePermitId.Value, "Repeat violations.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargePermitStatus.Revoked));
    }

    [Fact]
    public async Task Revoke_WhenAlreadyRevoked_ReturnsFailureRatherThanThrowing()
    {
        var permit = IssuedPermit();
        permit.Revoke("first revocation", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargePermitId>(), Arg.Any<CancellationToken>()).Returns(permit);
        var sut = new RevokeDischargePermitCommandHandler(_repo, NullLogger<RevokeDischargePermitCommandHandler>());
        var command = new RevokeDischargePermitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            permit.DischargePermitId.Value, "second attempt", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        // The handler's catch-all around the domain invariant violation must surface as a Result
        // failure, not an unhandled exception -- resilience: a single bad request must not crash
        // the process or leak an internal exception message to the caller.
        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("RevokeFailed");
    }
}
