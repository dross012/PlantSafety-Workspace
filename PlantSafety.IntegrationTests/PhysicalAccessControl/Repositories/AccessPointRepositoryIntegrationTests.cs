// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PhysicalAccessControl
//  Layer:          IntegrationTests
//  File:           AccessPointRepositoryIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the AccessPoint aggregate --
//                  exercises the real, DI-resolved IAccessPointRepository (AccessPointDapperRepository)
//                  against the real [PhysicalAccessControl].[AccessPoints] table and
//                  usp_AccessPoints_Create/Update/GetById/GetByIdItem stored procedures applied to
//                  the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyPhysicalAccessControlSchemaAsync).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using AccessPointAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint;

namespace PlantSafety.IntegrationTests.PhysicalAccessControl.Repositories;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AccessPointRepositoryIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ShouldRoundTripThroughRealStoredProcedures()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessPointRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var zoneId = Guid.NewGuid();
        var aggregate = AccessPointAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId,
            accessPointCode: $"AP-IT-{Guid.NewGuid():N}"[..12],
            description: "Integration test access point",
            deviceType: AccessPointDeviceType.MantrapDoor,
            safetyZoneId: zoneId,
            isPerimeter: true);

        await repository.AddAsync(aggregate, CancellationToken.None);

        var fetched = await repository.GetByIdAsync(aggregate.AccessPointId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.AccessPointId.Should().Be(aggregate.AccessPointId);
        fetched.Status.Should().Be(AccessPointStatus.Registered);
        fetched.DeviceType.Should().Be(AccessPointDeviceType.MantrapDoor);
        fetched.SafetyZoneId.Should().Be(zoneId);
        fetched.IsPerimeter.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessPointRepository>();

        var fetched = await repository.GetByIdAsync(
            PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids.AccessPointId.New(), CancellationToken.None);

        fetched.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_AfterLifecycleTransition_ShouldPersistNewStatus()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessPointRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var actor = PerformedByActor.User(createdBy);
        var aggregate = AccessPointAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()), actor,
            tenantId: TenantId, accessPointCode: $"AP-IT-{Guid.NewGuid():N}"[..12], description: "Lifecycle test");

        await repository.AddAsync(aggregate, CancellationToken.None);

        aggregate.Activate(UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(aggregate, CancellationToken.None);

        var afterActivate = await repository.GetByIdAsync(aggregate.AccessPointId, CancellationToken.None);
        afterActivate!.Status.Should().Be(AccessPointStatus.Active);

        afterActivate.Decommission(UtcTimestamp.Now(), actor, new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repository.UpdateAsync(afterActivate, CancellationToken.None);

        var afterDecommission = await repository.GetByIdAsync(aggregate.AccessPointId, CancellationToken.None);
        afterDecommission!.Status.Should().Be(AccessPointStatus.Decommissioned);
    }

    [Fact]
    public async Task ReadModel_GetByIdAsync_ShouldReturnDtoMatchingPersistedAggregate()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepository = scope.ServiceProvider.GetRequiredService<IAccessPointRepository>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IAccessPointReadRepository>();

        var createdBy = UserId.FromGuid(Guid.NewGuid());
        var aggregate = AccessPointAggregate.Create(
            createdBy, UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(createdBy),
            tenantId: TenantId, accessPointCode: $"AP-IT-{Guid.NewGuid():N}"[..12], description: "Read-model test");

        await writeRepository.AddAsync(aggregate, CancellationToken.None);

        var dto = await readRepository.GetByIdAsync(TenantId, aggregate.AccessPointId.Value, CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.AccessPointCode.Should().Be(aggregate.AccessPointCode);
        dto.Status.Should().Be("Registered");
    }
}
