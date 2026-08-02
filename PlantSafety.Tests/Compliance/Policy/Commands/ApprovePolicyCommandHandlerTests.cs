// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Policy.Commands
//  Layer:          Tests (Unit)
//  File:           ApprovePolicyCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Policy full build-out.
//                  ApprovePolicyCommandHandler must forward the caller's ETag to
//                  IPolicyRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Policy.Commands;
using PlantSafety.Domain.Compliance.Policy.Interfaces;
using PlantSafety.Domain.Compliance.Policy.ValueObjects.Ids;
using PolicyAggregate = PlantSafety.Domain.Compliance.Policy.Aggregates.Policy;

namespace PlantSafety.Tests.Compliance.Policy.Commands;

public sealed class ApprovePolicyCommandHandlerTests : UnitTestBase
{
    private readonly IPolicyRepository _repo = Substitute.For<IPolicyRepository>();

    private ApprovePolicyCommandHandler Sut() => new(_repo, NullLogger<ApprovePolicyCommandHandler>());

    private static PolicyAggregate CreatedPolicy() =>
        PolicyAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            policyTitle: "Confined Space Entry Policy",
            category: "ConfinedSpace",
            ownerId: TestUserId);

    private static ApprovePolicyCommand ApproveCommandFor(PolicyAggregate policy, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PolicyId: policy.PolicyId.Value,
            ApprovedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var policy = CreatedPolicy();
        _repo.GetByIdAsync(Arg.Any<PolicyId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(policy);

        var command = ApproveCommandFor(policy, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(policy, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var policy = CreatedPolicy();
        _repo.GetByIdAsync(Arg.Any<PolicyId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(policy);
        _repo.UpdateAsync(Arg.Any<PolicyAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Policy was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(policy, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
