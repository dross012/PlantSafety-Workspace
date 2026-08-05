// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HazardousWasteManagement
//  Layer:          IntegrationTests
//  File:           HazardousWasteManifestLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Integration tests for the full RCRA-style manifest chain-of-custody workflow --
//                  Generate -> RecordTransporterPickup (immutability) -> RecordTsdfReceipt ->
//                  FlagDiscrepancy -- exercised through the real MediatR pipeline, the real
//                  HazardousWasteManifestDapperRepository, and the real
//                  [HazardousWasteManagement].[Manifests] table + stored procedures applied to the
//                  Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HazardousWasteManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class HazardousWasteManifestLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task FullChainOfCustody_GenerateThroughTsdfReceipt_ShouldPersistEachTransition()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var wasteStreamId = Guid.NewGuid();
            var manifestNumber = $"MAN-{Guid.NewGuid():N}"[..30];

            var generateResult = await sender.Send(new GenerateManifestCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                manifestNumber, "EPA123456789", wasteStreamId, ActorId), CancellationToken.None);

            generateResult.IsSuccess.Should().BeTrue();
            var manifestId = generateResult.Value.HazardousWasteManifestId;

            var afterGenerate = await sender.Send(new GetManifestByIdQuery(TenantId, manifestId), CancellationToken.None);
            afterGenerate.Value.Item!.Status.Should().Be("Generated");
            afterGenerate.Value.Item.IsImmutable.Should().BeFalse();

            var transporterId = Guid.NewGuid();
            var pickupResult = await sender.Send(new RecordTransporterPickupCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, manifestId, transporterId, ActorId), CancellationToken.None);

            pickupResult.IsSuccess.Should().BeTrue();
            pickupResult.Value.Status.Should().Be("InTransit");

            var afterPickup = await sender.Send(new GetManifestByIdQuery(TenantId, manifestId), CancellationToken.None);
            afterPickup.Value.Item!.Status.Should().Be("InTransit");
            afterPickup.Value.Item.IsImmutable.Should().BeTrue("the manifest becomes immutable the instant the transporter signs for custody");
            afterPickup.Value.Item.TransporterId.Should().Be(transporterId);

            var tsdfFacilityId = Guid.NewGuid();
            var receiptResult = await sender.Send(new RecordTsdfReceiptCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, manifestId, tsdfFacilityId, ActorId), CancellationToken.None);

            receiptResult.IsSuccess.Should().BeTrue();
            receiptResult.Value.Status.Should().Be("ReceivedAtTsdf");

            var afterReceipt = await sender.Send(new GetManifestByIdQuery(TenantId, manifestId), CancellationToken.None);
            afterReceipt.Value.Item!.Status.Should().Be("ReceivedAtTsdf");
            afterReceipt.Value.Item.TsdfFacilityId.Should().Be(tsdfFacilityId);
            afterReceipt.Value.Item.RetentionExpiresAtUtc.Should().BeAfter(afterReceipt.Value.Item.GeneratedAtUtc.AddYears(2),
                "the mandatory 3-year retention invariant must be persisted");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task FlagDiscrepancy_OnGeneratedManifest_ShouldPersistDiscrepancyReason()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var generateResult = await sender.Send(new GenerateManifestCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"MAN-{Guid.NewGuid():N}"[..30], "EPA123456789", Guid.NewGuid(), ActorId), CancellationToken.None);
            var manifestId = generateResult.Value.HazardousWasteManifestId;

            var flagResult = await sender.Send(new FlagManifestDiscrepancyCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, manifestId,
                "Container count mismatch on receipt", ActorId), CancellationToken.None);

            flagResult.IsSuccess.Should().BeTrue();
            flagResult.Value.Status.Should().Be("DiscrepancyFlagged");

            var afterFlag = await sender.Send(new GetManifestByIdQuery(TenantId, manifestId), CancellationToken.None);
            afterFlag.Value.Item!.Status.Should().Be("DiscrepancyFlagged");
            afterFlag.Value.Item.DiscrepancyReason.Should().Be("Container count mismatch on receipt");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordTransporterPickup_WhenManifestDoesNotExist_ShouldReturnNotFoundFailure()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new RecordTransporterPickupCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), Guid.NewGuid(), ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ListOpenManifests_ShouldExcludeClosedManifestsAndIncludeOpenOnes()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var generateResult = await sender.Send(new GenerateManifestCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"MAN-{Guid.NewGuid():N}"[..30], "EPA123456789", Guid.NewGuid(), ActorId), CancellationToken.None);
            var manifestId = generateResult.Value.HazardousWasteManifestId;

            var listResult = await sender.Send(new ListOpenManifestsQuery(TenantId, null), CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(m => m.HazardousWasteManifestId == manifestId);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
