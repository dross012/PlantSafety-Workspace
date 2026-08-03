// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Enterprise.EventMesh
//  Layer:          Tests (Unit)
//  File:           RebuildWorkOrderTimelineQueryHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.4 Replay/Rebuild Across Systems, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Verifies WorkOrder state reconstruction by replaying a raw, chronologically
//                  ordered Outbox event stream: full lifecycle replay, point-in-time cutoff via
//                  AsOfUtc, and the "no events" failure path. Isolated, deterministic, no I/O.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.Operational.WorkOrders.Handlers;
using PlantSafety.Application.Operational.WorkOrders.Queries;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Tests.Enterprise.EventMesh;

public sealed class RebuildWorkOrderTimelineQueryHandlerTests : UnitTestBase
{
    private readonly IWorkOrderEventReplayRepository _repository = Substitute.For<IWorkOrderEventReplayRepository>();

    private RebuildWorkOrderTimelineQueryHandler Sut() => new(_repository, NullLogger<RebuildWorkOrderTimelineQueryHandler>());

    private static string Json(object o) => JsonSerializer.Serialize(o);

    [Fact]
    public async Task Handle_ReplaysFullLifecycle_ReconstructsCompletedState()
    {
        var workOrderId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var created = FixedUtcNow;
        var assigned = FixedUtcNow.AddHours(1);
        var started = FixedUtcNow.AddHours(2);
        var completed = FixedUtcNow.AddHours(3);

        var events = new List<WorkOrderReplayEventRecord>
        {
            new("WorkOrderCreated", created, Json(new { workOrderId })),
            new("WorkOrderAssigned", assigned, Json(new { workOrderId, technicianId })),
            new("WorkOrderStatusChanged", started, Json(new { workOrderId, newStatus = "inProgress" })),
            new("WorkOrderCompleted", completed, Json(new { workOrderId, completionNotes = "Replaced bearing." })),
        };

        _repository.GetRawEventsAsync(TestTenantId.Value, workOrderId, Arg.Any<CancellationToken>()).Returns(events);

        var result = await Sut().Handle(new RebuildWorkOrderTimelineQuery(TestTenantId.Value, workOrderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        result.Value.CreatedAtUtc.Should().Be(created);
        result.Value.LastAssignedTechnicianId.Should().Be(technicianId);
        result.Value.CompletionNotes.Should().Be("Replaced bearing.");
        result.Value.ReplayedEvents.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_WithAsOfCutoffBeforeCompletion_ReconstructsInProgressStateOnly()
    {
        var workOrderId = Guid.NewGuid();
        var created = FixedUtcNow;
        var started = FixedUtcNow.AddHours(2);
        var completed = FixedUtcNow.AddHours(3);

        var events = new List<WorkOrderReplayEventRecord>
        {
            new("WorkOrderCreated", created, Json(new { workOrderId })),
            new("WorkOrderStatusChanged", started, Json(new { workOrderId, newStatus = "inProgress" })),
            new("WorkOrderCompleted", completed, Json(new { workOrderId, completionNotes = "too late" })),
        };

        _repository.GetRawEventsAsync(TestTenantId.Value, workOrderId, Arg.Any<CancellationToken>()).Returns(events);

        var result = await Sut().Handle(
            new RebuildWorkOrderTimelineQuery(TestTenantId.Value, workOrderId, AsOfUtc: FixedUtcNow.AddHours(2).AddMinutes(30)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("inProgress");
        result.Value.CompletionNotes.Should().BeNull();
        result.Value.ReplayedEvents.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoEvents_ReturnsFailure()
    {
        var workOrderId = Guid.NewGuid();
        _repository.GetRawEventsAsync(TestTenantId.Value, workOrderId, Arg.Any<CancellationToken>())
            .Returns(new List<WorkOrderReplayEventRecord>());

        var result = await Sut().Handle(new RebuildWorkOrderTimelineQuery(TestTenantId.Value, workOrderId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
