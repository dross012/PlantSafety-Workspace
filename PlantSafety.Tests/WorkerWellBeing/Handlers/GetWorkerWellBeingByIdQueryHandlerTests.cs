// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.WorkerWellBeing.Handlers
//  Layer:          Tests (Unit)
//  File:           GetWorkerWellBeingByIdQueryHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetWorkerWellBeingByIdQueryHandler -- Phase 4 WorkerWellBeing
//                  full build-out. Covers: successful retrieval with ETag projection, not-found
//                  behavior with correct severity, and tenant-isolation. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.WorkerWellBeing.Handlers;
using PlantSafety.Application.WorkerWellBeing.Models;
using PlantSafety.Application.WorkerWellBeing.Queries;
using PlantSafety.Application.WorkerWellBeing.Repositories;

namespace PlantSafety.Tests.WorkerWellBeing.Handlers;

public sealed class GetWorkerWellBeingByIdQueryHandlerTests : UnitTestBase
{
    private readonly IWorkerWellBeingRepository _repo = Substitute.For<IWorkerWellBeingRepository>();

    private GetWorkerWellBeingByIdQueryHandler Sut() => new(_repo, NullLogger<GetWorkerWellBeingByIdQueryHandler>());

    private static WorkerWellBeingItemDto WorkerWellBeingFor(Guid workerWellBeingId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        WorkerWellBeingId = workerWellBeingId,
        TenantId = tenantId,
        WorkerId = TestUserId.Value,
        WellBeingRiskLevel = "Low",
        CheckInType = "MentalHealth",
        WellBeingScore = 7,
        FollowUpRequired = false,
        Status = "Created",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenWorkerWellBeingExistsForTenant_ReturnsItemWithEtag()
    {
        var workerWellBeingId = Guid.NewGuid();
        var workerWellBeing = WorkerWellBeingFor(workerWellBeingId, TestTenantId.Value);
        _repo.GetByIdAsync(workerWellBeingId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(workerWellBeing);

        var result = await Sut().Handle(new GetWorkerWellBeingByIdQuery(TestTenantId.Value, workerWellBeingId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.WorkerWellBeingId.Should().Be(workerWellBeingId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenWorkerWellBeingDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WorkerWellBeingItemDto?)null);

        var result = await Sut().Handle(new GetWorkerWellBeingByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("WorkerWellBeing.WorkerWellBeing.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing WorkerWellBeing record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenWorkerWellBeingBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_WorkerWellBeingAggregate_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for a record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (WorkerWellBeingId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var workerWellBeingId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(workerWellBeingId, otherTenantId, Arg.Any<CancellationToken>()).Returns(WorkerWellBeingFor(workerWellBeingId, otherTenantId));

        var result = await Sut().Handle(new GetWorkerWellBeingByIdQuery(TestTenantId.Value, workerWellBeingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("WorkerWellBeing.WorkerWellBeing.NotFound");
    }
}
