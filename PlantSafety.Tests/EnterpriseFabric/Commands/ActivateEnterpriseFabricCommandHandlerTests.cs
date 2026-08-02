// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnterpriseFabric.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateEnterpriseFabricCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 EnterpriseFabric full build-out.
//                  ActivateEnterpriseFabricCommandHandler must forward the caller's ETag to
//                  IEnterpriseFabricRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.EnterpriseFabric.Commands;
using PlantSafety.Domain.EnterpriseFabric.Repositories;
using PlantSafety.Domain.EnterpriseFabric.ValueObjects;
using EnterpriseFabricAggregate = PlantSafety.Domain.EnterpriseFabric.Aggregates.EnterpriseFabric;

namespace PlantSafety.Tests.EnterpriseFabric.Commands;

public sealed class ActivateEnterpriseFabricCommandHandlerTests : UnitTestBase
{
    private readonly IEnterpriseFabricRepository _repo = Substitute.For<IEnterpriseFabricRepository>();

    private ActivateEnterpriseFabricCommandHandler Sut() => new(_repo, NullLogger<ActivateEnterpriseFabricCommandHandler>());

    private static EnterpriseFabricAggregate CreatedFabric() =>
        EnterpriseFabricAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            fabricArea: "IntegrationHub",
            systemReference: "SAP-ERP",
            integrationType: "Api",
            ownerUserId: TestUserId);

    private static ActivateEnterpriseFabricCommand ActivateCommandFor(EnterpriseFabricAggregate fabric, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EnterpriseFabricId: fabric.EnterpriseFabricId.Value,
            ActorUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var fabric = CreatedFabric();
        _repo.GetByIdAsync(Arg.Any<EnterpriseFabricId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(fabric);

        var command = ActivateCommandFor(fabric, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(fabric, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var fabric = CreatedFabric();
        _repo.GetByIdAsync(Arg.Any<EnterpriseFabricId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(fabric);
        _repo.UpdateAsync(Arg.Any<EnterpriseFabricAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("EnterpriseFabric was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(fabric, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEnterpriseFabricNotFound_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<EnterpriseFabricId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EnterpriseFabricAggregate?)null);

        var command = new ActivateEnterpriseFabricCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EnterpriseFabricId: Guid.NewGuid(),
            ActorUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnterpriseFabric.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EnterpriseFabric record must map to HTTP 404, not the default 500");
    }
}
