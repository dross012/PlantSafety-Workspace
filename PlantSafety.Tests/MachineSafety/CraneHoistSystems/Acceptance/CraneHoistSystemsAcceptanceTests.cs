// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Acceptance
//  Layer:          Tests (Acceptance / UAT)
//  File:           CraneHoistSystemsAcceptanceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Given/When/Then acceptance (UAT) tests expressing the CraneHoistSystems
//                  module's documented OSHA 1910.179/1926.1400-series business rules from the
//                  perspective of a safety officer/crane operator end user, using the real
//                  Application-layer command handlers end-to-end. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Acceptance;

public sealed class CraneHoistSystemsAcceptanceTests : UnitTestBase
{
    [Fact]
    public async Task Given_NewCraneAsset_When_SafetyOfficerRegistersIt_Then_AssetIsVisibleInTheFleetRegistry()
    {
        // Given a safety officer wants to add a new overhead bridge crane to the fleet registry.
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

        // When they register it with its rated load capacity.
        var result = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "UAT-BAY-1", "Bay 1 Overhead Crane", "OverheadBridgeCrane", 8000m, TestUserId.Value), CancellationToken.None);

        // Then the asset is registered and immediately queryable from the fleet registry.
        result.IsSuccess.Should().BeTrue();
        var byId = await new PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistAssetByIdQueryHandler(
                repo, NullLogger<PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistAssetByIdQueryHandler>())
            .Handle(new PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries.GetCraneHoistAssetByIdQuery(
                TestTenantId.Value, result.Value.CraneHoistSystemsId), CancellationToken.None);
        byId.Value!.SystemStatus.Should().Be("Registered");
    }

    [Fact]
    public async Task Given_AssetInService_When_OperatorRecordsAnOverloadLift_Then_SafetyOfficeSeesTheOverloadFlagRaised()
    {
        // Given an in-service crane asset.
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "UAT-BAY-2", "Bay 2 Overhead Crane", "OverheadBridgeCrane", 4000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        // When the operator lifts a load exceeding the rated capacity.
        var overloadHandler = new RecordOverloadEventCommandHandler(repo, NullLogger<RecordOverloadEventCommandHandler>());
        await overloadHandler.Handle(new RecordOverloadEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, 4800m, TestUserId.Value), CancellationToken.None);

        // Then the safety office can see the overload count on the asset record.
        var byId = await new PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistAssetByIdQueryHandler(
                repo, NullLogger<PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistAssetByIdQueryHandler>())
            .Handle(new PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries.GetCraneHoistAssetByIdQuery(TestTenantId.Value, craneId), CancellationToken.None);
        byId.Value!.OverloadEventCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnqualifiedCraneNeedsAnOperator_When_TrainingIsCompleted_Then_OperatorIsQualifiedWithAnExpiryDate()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "UAT-BAY-3", "Bay 3 Gantry Crane", "GantryCrane", 3000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var qualifyHandler = new QualifyOperatorCommandHandler(repo, NullLogger<QualifyOperatorCommandHandler>());
        var expiry = FixedUtcNow.AddYears(1);
        var qualifyResult = await qualifyHandler.Handle(new QualifyOperatorCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, TestUserId.Value, expiry, TestUserId.Value), CancellationToken.None);

        qualifyResult.IsSuccess.Should().BeTrue();

        var expiring = await new PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.ListExpiringOperatorQualificationsQueryHandler(
                repo, NullLogger<PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.ListExpiringOperatorQualificationsQueryHandler>())
            .Handle(new PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries.ListExpiringOperatorQualificationsQuery(TestTenantId.Value, null, 400), CancellationToken.None);
        expiring.Value.Should().ContainSingle(a => a.CraneHoistSystemsId == craneId);
    }

    [Fact]
    public async Task Given_RetiredAsset_When_AttemptingFurtherOperationalCommands_Then_TheyAreRejected()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var asset = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "UAT-BAY-4", "Bay 4 Mobile Crane", CraneHoistAssetType.MobileCrane, 6000m);
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        await repo.AddAsync(asset, CancellationToken.None);

        var liftHandler = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());
        var result = await liftHandler.Handle(new RecordLiftEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, 500m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue("a retired asset must reject further operational commands per the module's lifecycle rules");
    }
}
