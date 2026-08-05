// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Functional
//  Layer:          Tests (Functional, black-box against the Application boundary)
//  File:           CraneHoistSystemsFunctionalTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Black-box functional tests verifying each CraneHoistSystems command/query
//                  request-response contract in isolation (distinct from the mock-based Commands/
//                  Handlers unit tests, which assert internal collaboration with the repository).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Functional;

public sealed class CraneHoistSystemsFunctionalTests : UnitTestBase
{
    private readonly FakeCraneHoistSystemsRepository _repo = new();

    private async Task<Guid> RegisterAssetAsync()
    {
        var handler = new RegisterCraneHoistAssetCommandHandler(_repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var result = await handler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "CRANE-FUNC", "Functional Test Crane", "Hoist", 2500m, TestUserId.Value), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        return result.Value.CraneHoistSystemsId;
    }

    [Fact]
    public async Task RegisterCraneHoistAsset_ReturnsRegisteredStatus()
    {
        var handler = new RegisterCraneHoistAssetCommandHandler(_repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var result = await handler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "CRANE-FUNC-1", "Functional Crane One", "GantryCrane", 3000m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Registered");
        result.Value.CraneHoistSystemsId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task RecordLiftEvent_AgainstRegisteredAsset_Succeeds()
    {
        var craneId = await RegisterAssetAsync();
        var handler = new RecordLiftEventCommandHandler(_repo, NullLogger<RecordLiftEventCommandHandler>());

        var result = await handler.Handle(new RecordLiftEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, 800m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetCraneHoistAssetById_AfterRegistration_ReturnsMatchingRecord()
    {
        var craneId = await RegisterAssetAsync();
        var handler = new GetCraneHoistAssetByIdQueryHandler(_repo, NullLogger<GetCraneHoistAssetByIdQueryHandler>());

        var result = await handler.Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, craneId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AssetTag.Should().Be("CRANE-FUNC");
        result.Value.RatedLoadCapacityKg.Should().Be(2500m);
    }

    [Fact]
    public async Task GetCraneHoistSystems_WithDateRangeExcludingCreation_ReturnsEmpty()
    {
        await RegisterAssetAsync();
        var handler = new GetCraneHoistSystemsQueryHandler(_repo, NullLogger<GetCraneHoistSystemsQueryHandler>());

        var result = await handler.Handle(
            new GetCraneHoistSystemsQuery(TestTenantId.Value, null, FixedUtcNow.AddYears(-2), FixedUtcNow.AddYears(-1)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
