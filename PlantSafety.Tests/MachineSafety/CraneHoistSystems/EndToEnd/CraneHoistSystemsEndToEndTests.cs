// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.EndToEnd
//  Layer:          Tests (End-to-End, in-process)
//  File:           CraneHoistSystemsEndToEndTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    End-to-end tests exercising the full CraneHoistSystems lifecycle through the
//                  real Application-layer command/query handlers against an in-memory fake
//                  repository (Application boundary -> Domain aggregate -> read-model
//                  projection), covering all 8 behaviors and all 6 query handlers in a single
//                  continuous workflow. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.EndToEnd;

public sealed class CraneHoistSystemsEndToEndTests : UnitTestBase
{
    [Fact]
    public async Task FullLifecycle_RegisterThroughRetire_PersistsExpectedStateAtEachStep()
    {
        var repo = new FakeCraneHoistSystemsRepository();

        // 1. Register
        var registerResult = await new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>())
            .Handle(new RegisterCraneHoistAssetCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "CRANE-E2E", "End To End Crane", "OverheadBridgeCrane", 5000m, TestUserId.Value),
                CancellationToken.None);
        registerResult.IsSuccess.Should().BeTrue();
        var craneId = registerResult.Value.CraneHoistSystemsId;

        // 2. RecordLiftEvent
        var liftResult = await new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>())
            .Handle(new RecordLiftEventCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, 1200m, TestUserId.Value), CancellationToken.None);
        liftResult.IsSuccess.Should().BeTrue();

        // 3. RecordOverloadEvent
        var overloadResult = await new RecordOverloadEventCommandHandler(repo, NullLogger<RecordOverloadEventCommandHandler>())
            .Handle(new RecordOverloadEventCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, 5500m, TestUserId.Value), CancellationToken.None);
        overloadResult.IsSuccess.Should().BeTrue();

        // 4. ScheduleCraneInspection
        var scheduleResult = await new ScheduleCraneInspectionCommandHandler(repo, NullLogger<ScheduleCraneInspectionCommandHandler>())
            .Handle(new ScheduleCraneInspectionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, FixedUtcNow.AddDays(30), TestUserId.Value), CancellationToken.None);
        scheduleResult.IsSuccess.Should().BeTrue();
        scheduleResult.Value.Status.Should().Be("InspectionDue");

        // 5. CompleteCraneInspection
        var completeResult = await new CompleteCraneInspectionCommandHandler(repo, NullLogger<CompleteCraneInspectionCommandHandler>())
            .Handle(new CompleteCraneInspectionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, true, TestUserId.Value), CancellationToken.None);
        completeResult.IsSuccess.Should().BeTrue();
        completeResult.Value.Status.Should().Be("InService");

        // 6. QualifyOperator
        var operatorId = Guid.NewGuid();
        var qualifyResult = await new QualifyOperatorCommandHandler(repo, NullLogger<QualifyOperatorCommandHandler>())
            .Handle(new QualifyOperatorCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, operatorId, FixedUtcNow.AddYears(1), TestUserId.Value), CancellationToken.None);
        qualifyResult.IsSuccess.Should().BeTrue();

        // 7. RecordRiggingInspection
        var riggingResult = await new RecordRiggingInspectionCommandHandler(repo, NullLogger<RecordRiggingInspectionCommandHandler>())
            .Handle(new RecordRiggingInspectionCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, false, "clean", TestUserId.Value), CancellationToken.None);
        riggingResult.IsSuccess.Should().BeTrue();

        // 8. SuspendOperatorQualification
        var suspendResult = await new SuspendOperatorQualificationCommandHandler(repo, NullLogger<SuspendOperatorQualificationCommandHandler>())
            .Handle(new SuspendOperatorQualificationCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, "audit finding", TestUserId.Value), CancellationToken.None);
        suspendResult.IsSuccess.Should().BeTrue();

        // Verify final state via each of the 6 query handlers.
        var byId = await new GetCraneHoistAssetByIdQueryHandler(repo, NullLogger<GetCraneHoistAssetByIdQueryHandler>())
            .Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, craneId), CancellationToken.None);
        byId.IsSuccess.Should().BeTrue();
        byId.Value.Should().NotBeNull();
        byId.Value!.SystemStatus.Should().Be("InService");
        byId.Value.OverloadEventCount.Should().Be(1);

        // RegisterCraneHoistAssetCommandHandler stamps CreatedAtUtc with the real wall-clock
        // UtcTimestamp.Now() (not the frozen FixedUtcNow test clock), so the query range must
        // span real "now" rather than the frozen clock window used elsewhere in this file.
        var listResult = await new GetCraneHoistSystemsQueryHandler(repo, NullLogger<GetCraneHoistSystemsQueryHandler>())
            .Handle(new GetCraneHoistSystemsQuery(TestTenantId.Value, null, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(5)), CancellationToken.None);
        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Items.Should().ContainSingle(i => i.CraneHoistSystemsId == craneId);

        var liftEvents = await new ListLiftEventsByAssetQueryHandler(repo, NullLogger<ListLiftEventsByAssetQueryHandler>())
            .Handle(new ListLiftEventsByAssetQuery(TestTenantId.Value, craneId), CancellationToken.None);
        liftEvents.Value.Should().ContainSingle().Which.LastObservedLoadKg.Should().Be(5500m);

        var inspectionHistory = await new GetInspectionHistoryQueryHandler(repo, NullLogger<GetInspectionHistoryQueryHandler>())
            .Handle(new GetInspectionHistoryQuery(TestTenantId.Value, craneId), CancellationToken.None);
        inspectionHistory.Value.Should().ContainSingle();

        var dueForInspection = await new ListAssetsDueForInspectionQueryHandler(repo, NullLogger<ListAssetsDueForInspectionQueryHandler>())
            .Handle(new ListAssetsDueForInspectionQuery(TestTenantId.Value, null, FixedUtcNow), CancellationToken.None);
        dueForInspection.Value.Should().BeEmpty("the asset already passed its inspection and is InService");

        var expiringQualifications = await new ListExpiringOperatorQualificationsQueryHandler(repo, NullLogger<ListExpiringOperatorQualificationsQueryHandler>())
            .Handle(new ListExpiringOperatorQualificationsQuery(TestTenantId.Value, null, 3650), CancellationToken.None);
        expiringQualifications.Value.Should().BeEmpty("the operator qualification was suspended in the final step");
    }
}
