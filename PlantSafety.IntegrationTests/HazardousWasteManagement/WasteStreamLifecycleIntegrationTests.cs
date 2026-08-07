// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HazardousWasteManagement
//  Layer:          IntegrationTests
//  File:           WasteStreamLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Integration tests for the WasteStream lifecycle -- exercises the real MediatR
//                  pipeline (CharacterizeWasteStreamCommand -> GetWasteStreamByIdQuery), the real
//                  WasteStreamDapperRepository, and the real [HazardousWasteManagement].[WasteStreams]
//                  table + usp_WasteStreams_Create/GetById stored procedures applied to the
//                  Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyHazardousWasteManagementSchemaAsync).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HazardousWasteManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class WasteStreamLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Characterize_ThenGetById_ShouldRoundTripRealData()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var characterizeResult = await sender.Send(new CharacterizeWasteStreamCommand(
                CommandId: Guid.NewGuid(),
                CorrelationId: CorrelationId,
                CausationId: Guid.NewGuid(),
                TenantId: TenantId,
                SiteId: null,
                StreamName: "Spent Solvent Stream",
                EpaWasteCode: "D001",
                HazardClass: "Ignitable",
                GeneratorEpaId: "EPA123456789",
                PerformedByUserId: ActorId), CancellationToken.None);

            characterizeResult.IsSuccess.Should().BeTrue("CharacterizeWasteStreamCommand must succeed against the real Testcontainers schema");
            var wasteStreamId = characterizeResult.Value.WasteStreamId;

            var getResult = await sender.Send(new GetWasteStreamByIdQuery(TenantId, wasteStreamId), CancellationToken.None);

            getResult.IsSuccess.Should().BeTrue();
            getResult.Value.Item.Should().NotBeNull("the just-characterized WasteStream must be retrievable through the real stored procedure");
            getResult.Value.Item!.WasteStreamId.Should().Be(wasteStreamId);
            getResult.Value.Item.TenantId.Should().Be(TenantId);
            getResult.Value.Item.StreamName.Should().Be("Spent Solvent Stream");
            getResult.Value.Item.EpaWasteCode.Should().Be("D001");
            getResult.Value.Item.HazardClass.Should().Be("Ignitable");
            getResult.Value.Item.Status.Should().Be("Characterized");
            getResult.Value.Item.CharacterizationVersion.Should().Be(1);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetById_WhenWasteStreamDoesNotExist_ShouldReturnSuccessWithNullItem()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetWasteStreamByIdQuery(TenantId, Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Item.Should().BeNull();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetById_ScopedToDifferentTenant_ShouldNotLeakAcrossTenants()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var characterizeResult = await sender.Send(new CharacterizeWasteStreamCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "Tenant-Scoped Stream", "D002", "Corrosive", "EPA000000001", ActorId), CancellationToken.None);
            var wasteStreamId = characterizeResult.Value.WasteStreamId;

            // A real cross-tenant read can no longer be exercised through ISender: with the
            // zero-trust tenant isolation now enforced by TenantIsolationBehavior, any request
            // whose bound TenantId doesn't equal the authenticated context's tenant is rejected
            // with ApplicationAuthorizationException before the handler ever runs -- that IS the
            // "must not leak across tenants" guarantee at the pipeline level. What this test can
            // still verify directly is the repository/stored-procedure's own defense-in-depth
            // TenantId filter, by resolving IWasteStreamRepository straight from the DI container
            // (bypassing MediatR, matching the precedent in
            // CognitiveLoadAnalyticsSecurityResilienceTests.GetByIdAsync_WhenLedgerBelongsTo
            // DifferentTenant_ReturnsNullRatherThanLeakingSensitiveData).
            var repository = scope.ServiceProvider.GetRequiredService<
                PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories.IWasteStreamRepository>();

            var otherTenantId = Guid.NewGuid();
            var crossTenantItem = await repository.GetWasteStreamByIdAsync(otherTenantId, wasteStreamId, CancellationToken.None);

            crossTenantItem.Should().BeNull("a WasteStream record must not be visible to a different tenant");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
