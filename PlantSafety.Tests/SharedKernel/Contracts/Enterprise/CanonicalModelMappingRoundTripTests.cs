// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelMappingRoundTripTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Provider contract validation: constructs a real aggregate from each of the six
//                  grounding modules via its public factory, maps it to the enterprise canonical
//                  model via that module's own Application-layer mapper, and asserts the key
//                  identity/status/timestamp fields round-trip correctly. Isolated, deterministic,
//                  no I/O, no network.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.CAPA.Mappings;
using PlantSafety.Application.CrossDomain.Machines.Mappings;
using PlantSafety.Application.CrossDomain.Machines.Models;
using PlantSafety.Application.Operational.ProductionTracking.Mappings;
using PlantSafety.Application.Operational.WorkOrders.Mappings;
using PlantSafety.Application.RiskManagement.Mappings;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Mappings;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.Domain.Operational.WorkOrders.Enums;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Provider contract validation: real aggregate -&gt; canonical DTO round-trip for every grounding module.</summary>
public sealed class CanonicalModelMappingRoundTripTests : UnitTestBase
{
    [Fact]
    public void WorkOrder_ToCanonicalV1_ShouldRoundTripKeyFields()
    {
        var machineId = Guid.NewGuid();
        var workOrder = WorkOrder.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.NewGuid(), siteId: Guid.NewGuid(),
            workOrderCode: "WO-042", machineId: machineId, machineCode: "M-007",
            priority: WorkOrderPriorityLevel.High, description: "Replace bearing",
            dueAtUtc: FixedTimestamp);

        var canonical = workOrder.ToCanonicalV1();

        canonical.WorkOrderId.Should().Be(workOrder.WorkOrderId.Value);
        canonical.TenantId.Should().Be(workOrder.TenantId);
        canonical.SiteId.Should().Be(workOrder.SiteId);
        canonical.WorkOrderCode.Should().Be("WO-042");
        canonical.AssetId.Should().Be(machineId);
        canonical.AssetCode.Should().Be("M-007");
        canonical.Priority.Should().Be(WorkOrderPriorityLevel.High.ToString());
        canonical.Status.Should().Be(workOrder.Status.ToString());
        canonical.DueAtUtc.Should().Be(FixedTimestamp);
        canonical.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void WorkOrder_ToCanonicalV2_ShouldRoundTripAssignmentDetail()
    {
        var workOrder = WorkOrder.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.NewGuid(), siteId: null,
            workOrderCode: "WO-043", machineId: Guid.NewGuid(), machineCode: "M-008",
            priority: WorkOrderPriorityLevel.Medium, description: "Inspect wiring",
            dueAtUtc: FixedTimestamp);

        var technicianId = Guid.NewGuid();
        workOrder.Assign(technicianId, "J. Rivera", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var canonical = workOrder.ToCanonicalV2();

        canonical.TechnicianId.Should().Be(technicianId);
        canonical.TechnicianName.Should().Be("J. Rivera");
        canonical.AssignedAtUtc.Should().Be(FixedTimestamp);
        canonical.Status.Should().Be(WorkOrderStatus.Assigned.ToString());
    }

    [Fact]
    public void ProductionRun_ToCanonicalV1_ShouldRoundTripKeyFields()
    {
        var lineId = ProductionLineId.New();
        var machineId = Guid.NewGuid();
        var run = ProductionRun.Start(
            TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId,
            tenantId: Guid.NewGuid(), siteId: Guid.NewGuid(),
            runCode: "PR-100", lineId: lineId, lineName: "Line 1",
            machineId: machineId, operatorName: "A. Chen", idealCycleTimeSeconds: 10.0);

        var canonical = run.ToCanonicalV1();

        canonical.ProductionOrderId.Should().Be(run.ProductionRunId.Value);
        canonical.OrderCode.Should().Be("PR-100");
        canonical.ProductionLineId.Should().Be(lineId.Value);
        canonical.ProductionLineName.Should().Be("Line 1");
        canonical.AssetId.Should().Be(machineId);
        canonical.Status.Should().Be(run.Status.ToString());
        canonical.StartedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Risk_ToCanonicalV1_ShouldRoundTripKeyFields()
    {
        var tenantId = Guid.NewGuid();
        var risk = Risk.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: tenantId, siteId: Guid.NewGuid(),
            riskTitle: "Pinch point on Conveyor B", location: "Building 2, Zone A",
            hazardSource: "Mechanical", ownerId: TestUserId);

        var canonical = risk.ToCanonicalV1();

        canonical.RiskEventId.Should().Be(risk.RiskId.Value);
        canonical.TenantId.Should().Be(tenantId);
        canonical.RiskTitle.Should().Be("Pinch point on Conveyor B");
        canonical.Location.Should().Be("Building 2, Zone A");
        canonical.HazardSource.Should().Be("Mechanical");
        canonical.Status.Should().Be(risk.Status.ToString());
        canonical.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Risk_ToCanonicalV3_ShouldDeriveOverdueProjection_FromRealNextReviewDueUtc()
    {
        var risk = Risk.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.NewGuid(), siteId: null,
            riskTitle: "Overheating motor", location: "Building 1", hazardSource: "Electrical",
            ownerId: TestUserId);
        risk.ScoreRisk(2, 2, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        risk.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        risk.ApplyControlMeasure(
            PlantSafety.Domain.RiskManagement.Enums.ControlHierarchyLevel.Elimination,
            "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        risk.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var farFuture = FixedUtcNow.AddYears(10);
        var canonical = risk.ToCanonicalV3(nowUtc: farFuture);

        canonical.NextReviewDueUtc.Should().Be(risk.NextReviewDueUtc);
        canonical.IsReviewOverdue.Should().BeTrue(because: "farFuture is long past the real NextReviewDueUtc computed by the aggregate");
    }

    [Fact]
    public void CorrectiveAction_ToCanonicalV1_ShouldRoundTripKeyFields()
    {
        var sourceEventId = Guid.NewGuid();
        var action = CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.NewGuid(), sourceEventId: sourceEventId, sourceEventType: "Incident",
            title: "Install machine guard", description: "Install a fixed guard.",
            actionType: "Corrective", priority: "High", assignedToWorkerId: TestUserId, dueAtUtc: FixedTimestamp);

        var canonical = action.ToCanonicalV1();

        canonical.ImprovementActionId.Should().Be(action.CorrectiveActionId.Value);
        canonical.SourceEventId.Should().Be(sourceEventId);
        canonical.SourceEventType.Should().Be("Incident");
        canonical.Title.Should().Be("Install machine guard");
        canonical.ActionType.Should().Be("Corrective");
        canonical.Priority.Should().Be("High");
        canonical.AssignedToWorkerId.Should().Be(TestUserId.Value);
        canonical.DueAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void CorrectiveAction_ToCanonicalV3_ShouldFlagOverdue_UsingSameConditionAsMarkOverdue()
    {
        var action = CorrectiveAction.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.NewGuid(), sourceEventId: Guid.NewGuid(), sourceEventType: "NearMiss",
            title: "Retrain crew", description: "Retrain crew on LOTO procedure.",
            actionType: "Preventive", priority: "Medium", assignedToWorkerId: TestUserId, dueAtUtc: FixedTimestamp);

        var pastDue = UtcTimestamp.From(FixedUtcNow.AddDays(30));
        var canonical = action.ToCanonicalV3(pastDue);

        canonical.IsPredictedOverdue.Should().BeTrue();
    }

    [Fact]
    public void OptimizationRun_ToCanonicalV1_ShouldProduceOneBlockPerCommittedAssignment()
    {
        var taskId = SchedulableTaskId.New();
        var resourceId = SchedulingResourceId.New();
        var task = new SchedulableTask(
            taskId, "PM-1 Conveyor B Inspection", TimeSpan.FromHours(2),
            FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(1)), basePriorityWeight: 1.0);
        var availability = new ResourceAvailabilityWindow(
            resourceId, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddDays(1)));

        var run = OptimizationRun.Request(
            tenantId: Guid.NewGuid(), siteId: Guid.NewGuid(),
            inputTasks: [task], inputAvailability: [availability],
            objective: OptimizationObjective.MinimizeMakespan, requestedBy: TestUserId,
            nowUtc: FixedTimestamp, correlationId: TestCorrelationId);

        run.Begin(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var assignment = new ScheduleAssignment(taskId, resourceId, FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)));
        run.Complete("GreedyOptimizationStrategy", [assignment], [], [], score: 0.9,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var blocks = run.ToCanonicalV1();

        blocks.Should().ContainSingle();
        blocks[0].OptimizationRunId.Should().Be(run.OptimizationRunId.Value);
        blocks[0].TaskId.Should().Be(taskId.Value);
        blocks[0].ResourceId.Should().Be(resourceId.Value);
        blocks[0].StartUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void MachinesItemDto_ToCanonicalV2_ShouldRoundTripKeyFields()
    {
        var machine = new MachinesItemDto
        {
            MachineId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            MachineName = "Conveyor B",
            MachineType = "Conveyor",
            SerialNumber = "SN-12345",
            Status = "Operational",
            LastServicedAtUtc = FixedUtcNow,
            LastServicedBy = Guid.NewGuid(),
        };

        var canonical = machine.ToCanonicalV2();

        canonical.AssetId.Should().Be(machine.MachineId);
        canonical.AssetName.Should().Be("Conveyor B");
        canonical.AssetType.Should().Be("Conveyor");
        canonical.SerialNumber.Should().Be("SN-12345");
        canonical.LastServicedBy.Should().Be(machine.LastServicedBy);
        canonical.Status.Should().Be("Operational");
    }
}
