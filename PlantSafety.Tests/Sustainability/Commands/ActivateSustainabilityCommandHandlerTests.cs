// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateSustainabilityCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Sustainability full build-out.
//                  ActivateSustainabilityCommandHandler must forward the caller's ETag to
//                  ISustainabilityRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Sustainability.Commands;
using PlantSafety.Domain.Sustainability.Repositories;
using PlantSafety.Domain.Sustainability.ValueObjects;
using SustainabilityAggregate = PlantSafety.Domain.Sustainability.Aggregates.Sustainability;

namespace PlantSafety.Tests.Sustainability.Commands;

public sealed class ActivateSustainabilityCommandHandlerTests : UnitTestBase
{
    private readonly ISustainabilityRepository _repo = Substitute.For<ISustainabilityRepository>();

    private ActivateSustainabilityCommandHandler Sut() => new(_repo, NullLogger<ActivateSustainabilityCommandHandler>());

    private static SustainabilityAggregate CreatedSustainability() =>
        SustainabilityAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            initiativeName: "Site-Wide Emissions Reduction",
            category: "Emissions",
            targetValue: 1000m,
            ownerUserId: TestUserId);

    private static ActivateSustainabilityCommand ActivateCommandFor(SustainabilityAggregate sustainability, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            SustainabilityId: sustainability.SustainabilityId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var sustainability = CreatedSustainability();
        _repo.GetByIdAsync(Arg.Any<SustainabilityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(sustainability);

        var command = ActivateCommandFor(sustainability, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(sustainability, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var sustainability = CreatedSustainability();
        _repo.GetByIdAsync(Arg.Any<SustainabilityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(sustainability);
        _repo.UpdateAsync(Arg.Any<SustainabilityAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Sustainability was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(sustainability, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenSustainabilityDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<SustainabilityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SustainabilityAggregate?)null);

        var command = new ActivateSustainabilityCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            SustainabilityId: Guid.NewGuid(),
            ActivatedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.Sustainability.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Sustainability record must map to HTTP 404, not the default 500");
    }
}
