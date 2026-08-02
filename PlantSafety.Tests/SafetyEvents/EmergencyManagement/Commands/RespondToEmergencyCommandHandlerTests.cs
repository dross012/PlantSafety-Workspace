// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.EmergencyManagement.Commands
//  Layer:          Tests (Unit)
//  File:           RespondToEmergencyCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.EmergencyManagement full
//                  build-out. RespondToEmergencyCommandHandler must forward the caller's ETag to
//                  IEmergencyManagementRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Commands;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids;
using EmergencyManagementAggregate = PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement;

namespace PlantSafety.Tests.SafetyEvents.EmergencyManagement.Commands;

public sealed class RespondToEmergencyCommandHandlerTests : UnitTestBase
{
    private readonly IEmergencyManagementRepository _repo = Substitute.For<IEmergencyManagementRepository>();

    private RespondToEmergencyCommandHandler Sut() => new(_repo, NullLogger<RespondToEmergencyCommandHandler>());

    private static EmergencyManagementAggregate DeclaredEmergency() =>
        EmergencyManagementAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            emergencyType: "Fire",
            severity: "Major",
            location: "Building A",
            casualtyCount: 0,
            responseTeamNotified: false);

    private static RespondToEmergencyCommand RespondCommandFor(EmergencyManagementAggregate emergency, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EmergencyManagementId: emergency.EmergencyManagementId.Value,
            RespondedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var emergency = DeclaredEmergency();
        _repo.GetByIdAsync(Arg.Any<EmergencyManagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(emergency);

        var command = RespondCommandFor(emergency, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(emergency, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var emergency = DeclaredEmergency();
        _repo.GetByIdAsync(Arg.Any<EmergencyManagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(emergency);
        _repo.UpdateAsync(Arg.Any<EmergencyManagementAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("EmergencyManagement was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = RespondCommandFor(emergency, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
