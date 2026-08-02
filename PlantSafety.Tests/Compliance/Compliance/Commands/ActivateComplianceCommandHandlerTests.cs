// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Compliance.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateComplianceCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Compliance (self-named) full
//                  build-out. ActivateComplianceCommandHandler must forward the caller's ETag to
//                  IComplianceRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. The aggregate type is referenced via the
//                  "ComplianceAggregate" alias throughout this file for the same
//                  collision-avoidance reason documented across the rest of this module's
//                  build-out. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Compliance.Commands;
using PlantSafety.Domain.Compliance.Compliance.Interfaces;
using PlantSafety.Domain.Compliance.Compliance.ValueObjects.Ids;
using ComplianceAggregate = PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance;

namespace PlantSafety.Tests.Compliance.Compliance.Commands;

public sealed class ActivateComplianceCommandHandlerTests : UnitTestBase
{
    private readonly IComplianceRepository _repo = Substitute.For<IComplianceRepository>();

    private ActivateComplianceCommandHandler Sut() => new(_repo, NullLogger<ActivateComplianceCommandHandler>());

    private static ComplianceAggregate CreatedCompliance() =>
        ComplianceAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            regulationReference: "29 CFR 1910.147",
            ruleDescription: "Lockout/tagout program compliance",
            responsiblePartyId: TestUserId);

    private static ActivateComplianceCommand ActivateCommandFor(ComplianceAggregate compliance, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ComplianceId: compliance.ComplianceId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var compliance = CreatedCompliance();
        _repo.GetByIdAsync(Arg.Any<ComplianceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(compliance);

        var command = ActivateCommandFor(compliance, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(compliance, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var compliance = CreatedCompliance();
        _repo.GetByIdAsync(Arg.Any<ComplianceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(compliance);
        _repo.UpdateAsync(Arg.Any<ComplianceAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Compliance was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(compliance, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
