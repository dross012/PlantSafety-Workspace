// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HazardousWasteManagement
//  Layer:          IntegrationTests
//  File:           DisposalCertificateLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Integration tests for recording a DisposalCertificate against a manifest and
//                  retrieving the disposal history for the manifest's waste stream -- exercised
//                  through the real MediatR pipeline, the real DisposalCertificateDapperRepository,
//                  and the real [HazardousWasteManagement].[DisposalCertificates] table
//                  (joined against [Manifests] to resolve WasteStreamId) applied to the
//                  Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HazardousWasteManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DisposalCertificateLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RecordDisposalCertificate_ThenGetHistoryByStream_ShouldResolveWasteStreamIdViaManifestJoin()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var wasteStreamId = Guid.NewGuid();

            var generateResult = await sender.Send(new GenerateManifestCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                $"MAN-{Guid.NewGuid():N}"[..30], "EPA123456789", wasteStreamId, ActorId), CancellationToken.None);
            var manifestId = generateResult.Value.HazardousWasteManifestId;

            var tsdfFacilityId = Guid.NewGuid();
            var recordResult = await sender.Send(new RecordDisposalCertificateCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                manifestId, tsdfFacilityId, "Incineration", FixedUtcNow, ActorId), CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Received");

            var historyResult = await sender.Send(new GetDisposalHistoryByStreamQuery(TenantId, wasteStreamId), CancellationToken.None);

            historyResult.IsSuccess.Should().BeTrue();
            var item = historyResult.Value.Items.Should().ContainSingle().Subject;
            item.ManifestId.Should().Be(manifestId);
            item.WasteStreamId.Should().Be(wasteStreamId, "usp_DisposalCertificates_GetHistoryByStream must resolve WasteStreamId via the Manifests join");
            item.TsdfFacilityId.Should().Be(tsdfFacilityId);
            item.DisposalMethod.Should().Be("Incineration");
            item.Status.Should().Be("Received");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetHistoryByStream_WithNoCertificates_ShouldReturnEmptyList()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetDisposalHistoryByStreamQuery(TenantId, Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
        }
        finally
        {
            scope.Dispose();
        }
    }
}
