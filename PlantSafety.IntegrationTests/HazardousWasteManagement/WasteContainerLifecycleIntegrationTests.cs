// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HazardousWasteManagement
//  Layer:          IntegrationTests
//  File:           WasteContainerLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Integration tests for the WasteContainer open/close lifecycle and the
//                  AccumulationArea-scoped list query -- exercised through the real MediatR
//                  pipeline, the real WasteContainerDapperRepository, and the real
//                  [HazardousWasteManagement].[WasteContainers] table + stored procedures applied
//                  to the Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HazardousWasteManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class WasteContainerLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OpenThenClose_ShouldRoundTripThroughListByAccumulationArea()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var accumulationAreaId = Guid.NewGuid();
            var wasteStreamId = Guid.NewGuid();

            var openResult = await sender.Send(new OpenAccumulationContainerCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                accumulationAreaId, wasteStreamId, "DRUM-INTEGRATION-001", 55m, 90, ActorId), CancellationToken.None);

            openResult.IsSuccess.Should().BeTrue();
            var containerId = openResult.Value.WasteContainerId;
            openResult.Value.Status.Should().Be("Open");

            var listAfterOpen = await sender.Send(new ListContainersByAccumulationAreaQuery(TenantId, accumulationAreaId), CancellationToken.None);
            listAfterOpen.IsSuccess.Should().BeTrue();
            var openItem = listAfterOpen.Value.Items.Should().ContainSingle(c => c.WasteContainerId == containerId).Subject;
            openItem.Status.Should().Be("Open");
            openItem.ContainerLabel.Should().Be("DRUM-INTEGRATION-001");
            openItem.CapacityGallons.Should().Be(55m);
            openItem.ClosedAtUtc.Should().BeNull();

            var closeResult = await sender.Send(new CloseAccumulationContainerCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, containerId, ActorId), CancellationToken.None);

            closeResult.IsSuccess.Should().BeTrue();
            closeResult.Value.Status.Should().Be("Closed");

            var listAfterClose = await sender.Send(new ListContainersByAccumulationAreaQuery(TenantId, accumulationAreaId), CancellationToken.None);
            var closedItem = listAfterClose.Value.Items.Should().ContainSingle(c => c.WasteContainerId == containerId).Subject;
            closedItem.Status.Should().Be("Closed");
            closedItem.ClosedAtUtc.Should().NotBeNull();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task Close_WhenContainerDoesNotExist_ShouldReturnNotFoundFailure()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new CloseAccumulationContainerCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteContainer.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ListByAccumulationArea_WithNoContainers_ShouldReturnEmptyList()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new ListContainersByAccumulationAreaQuery(TenantId, Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
        }
        finally
        {
            scope.Dispose();
        }
    }
}
