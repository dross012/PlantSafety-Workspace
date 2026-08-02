// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.PreventiveMaintenance.Handlers
//  Layer:          Tests (Unit)
//  File:           GetPreventiveMaintenanceTaskByIdQueryHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetPreventiveMaintenanceTaskByIdQueryHandler -- Phase 4
//                  Operational.PreventiveMaintenance PreventiveMaintenanceTask full build-out.
//                  Covers: successful retrieval with ETag projection, not-found behavior with
//                  correct severity, and tenant-isolation (a record belonging to a different
//                  tenant must surface as NotFound, never leaking cross-tenant existence). Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Handlers.GetPermitsByIdQueryHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Operational.PreventiveMaintenance.Handlers;
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;
using PlantSafety.Application.Operational.PreventiveMaintenance.Queries;
using PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

namespace PlantSafety.Tests.Operational.PreventiveMaintenance.Handlers;

public sealed class GetPreventiveMaintenanceTaskByIdQueryHandlerTests : UnitTestBase
{
    private readonly IPreventiveMaintenanceTaskRepository _repo = Substitute.For<IPreventiveMaintenanceTaskRepository>();

    private GetPreventiveMaintenanceTaskByIdQueryHandler Sut() => new(_repo, NullLogger<GetPreventiveMaintenanceTaskByIdQueryHandler>());

    private static PreventiveMaintenanceTaskItemDto TaskFor(Guid preventiveMaintenanceTaskId, Guid tenantId, string? eTag = "0x0000000000009ABC") => new()
    {
        PreventiveMaintenanceTaskId = preventiveMaintenanceTaskId,
        TenantId = tenantId,
        Status = "Scheduled",
        EquipmentId = Guid.NewGuid(),
        TaskDescription = "Lubricate bearings on Pump 3",
        ScheduledAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
        ETag = eTag,
    };

    [Fact]
    public async Task Handle_WhenTaskExistsForTenant_ReturnsItemWithEtag()
    {
        var taskId = Guid.NewGuid();
        var task = TaskFor(taskId, TestTenantId.Value);
        _repo.GetByIdAsync(taskId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(task);

        var result = await Sut().Handle(new GetPreventiveMaintenanceTaskByIdQuery(TestTenantId.Value, taskId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreventiveMaintenanceTaskId.Should().Be(taskId);
        result.Value.ETag.Should().Be("0x0000000000009ABC");
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PreventiveMaintenanceTaskItemDto?)null);

        var result = await Sut().Handle(new GetPreventiveMaintenanceTaskByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.PreventiveMaintenance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing PreventiveMaintenanceTask record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenTaskBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_PreventiveMaintenanceTask_GetById filters by TenantId
        // in SQL, so a lookup scoped to TestTenantId for a record that actually belongs to
        // another tenant returns no row at all -- the repository is not stubbed for that
        // (PreventiveMaintenanceTaskId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var taskId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(taskId, otherTenantId, Arg.Any<CancellationToken>()).Returns(TaskFor(taskId, otherTenantId));

        var result = await Sut().Handle(new GetPreventiveMaintenanceTaskByIdQuery(TestTenantId.Value, taskId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Operational.PreventiveMaintenance.NotFound");
    }
}
