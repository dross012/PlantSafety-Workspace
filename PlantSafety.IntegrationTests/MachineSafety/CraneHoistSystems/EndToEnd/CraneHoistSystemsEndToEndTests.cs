// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.MachineSafety.CraneHoistSystems
//  Layer:          IntegrationTests
//  File:           CraneHoistSystemsEndToEndTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    End-to-end integration test driving the full CraneHoistSystems lifecycle
//                  (Register -> RecordLiftEvent -> RecordOverloadEvent ->
//                  ScheduleCraneInspection -> CompleteCraneInspection -> QualifyOperator ->
//                  RecordRiggingInspection -> SuspendOperatorQualification) through the real
//                  MediatR pipeline (ISender) resolved from the DI container, backed by the real
//                  Testcontainers SQL Server database and the real CraneHoistSystemsDapperRepository
//                  / stored procedures -- no mocks anywhere in the path.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.MachineSafety.CraneHoistSystems.EndToEnd;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CraneHoistSystemsEndToEndTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FullLifecycle_ThroughRealMediatRPipelineAndRealDatabase_PersistsExpectedFinalState()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();

            var registerResult = await sender.Send(new RegisterCraneHoistAssetCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId,
                siteId, "E2E-INT-1", "Integration End-To-End Crane", "OverheadBridgeCrane", 6000m, ActorId), CancellationToken.None);
            registerResult.IsSuccess.Should().BeTrue();
            var craneId = registerResult.Value.CraneHoistSystemsId;

            var liftResult = await sender.Send(new RecordLiftEventCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, 3000m, ActorId), CancellationToken.None);
            liftResult.IsSuccess.Should().BeTrue();

            var overloadResult = await sender.Send(new RecordOverloadEventCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, 6500m, ActorId), CancellationToken.None);
            overloadResult.IsSuccess.Should().BeTrue();

            var scheduleResult = await sender.Send(new ScheduleCraneInspectionCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, DateTime.UtcNow.AddDays(30), ActorId), CancellationToken.None);
            scheduleResult.IsSuccess.Should().BeTrue();
            scheduleResult.Value.Status.Should().Be("InspectionDue");

            var completeResult = await sender.Send(new CompleteCraneInspectionCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, true, ActorId), CancellationToken.None);
            completeResult.IsSuccess.Should().BeTrue();
            completeResult.Value.Status.Should().Be("InService");

            var operatorId = Guid.NewGuid();
            var qualifyResult = await sender.Send(new QualifyOperatorCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, operatorId, DateTime.UtcNow.AddYears(1), ActorId), CancellationToken.None);
            qualifyResult.IsSuccess.Should().BeTrue();

            var riggingResult = await sender.Send(new RecordRiggingInspectionCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, false, "no defects found", ActorId), CancellationToken.None);
            riggingResult.IsSuccess.Should().BeTrue();

            var suspendResult = await sender.Send(new SuspendOperatorQualificationCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, craneId, "routine audit hold", ActorId), CancellationToken.None);
            suspendResult.IsSuccess.Should().BeTrue();

            // ---- Verify the fully-persisted final state through the real read-model queries. ----
            var byId = await sender.Send(new GetCraneHoistAssetByIdQuery(TenantId, craneId), CancellationToken.None);
            byId.IsSuccess.Should().BeTrue();
            byId.Value.Should().NotBeNull();
            byId.Value!.SystemStatus.Should().Be("InService");
            byId.Value.OverloadEventCount.Should().Be(1);
            byId.Value.LastObservedLoadKg.Should().Be(6500m);

            var listResult = await sender.Send(new GetCraneHoistSystemsQuery(
                TenantId, siteId, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(5)), CancellationToken.None);
            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.CraneHoistSystemsId == craneId);

            var inspectionHistory = await sender.Send(new GetInspectionHistoryQuery(TenantId, craneId), CancellationToken.None);
            inspectionHistory.Value.Should().ContainSingle();
        }
        finally
        {
            scope.Dispose();
        }
    }
}
