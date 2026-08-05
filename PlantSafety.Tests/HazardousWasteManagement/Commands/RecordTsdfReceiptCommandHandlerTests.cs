// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           RecordTsdfReceiptCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for RecordTsdfReceiptCommandHandler, including the
//                  ErrorSeverity.NotFound regression this test suite surfaced and fixed.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class RecordTsdfReceiptCommandHandlerTests : UnitTestBase
{
    private readonly IHazardousWasteManifestRepository _repo = Substitute.For<IHazardousWasteManifestRepository>();

    private RecordTsdfReceiptCommandHandler Sut() => new(_repo, NullLogger<RecordTsdfReceiptCommandHandler>());

    private static Agg InTransitManifest()
    {
        var manifest = Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "MAN-2026-000123", "EPA123456789", Guid.NewGuid());
        manifest.RecordTransporterPickup(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return manifest;
    }

    private static RecordTsdfReceiptCommand CommandFor(Guid manifestId, Guid tsdfFacilityId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        HazardousWasteManifestId: manifestId,
        TsdfFacilityId: tsdfFacilityId,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithInTransitManifest_RecordsReceiptAndReturnsSuccess()
    {
        var manifest = InTransitManifest();
        var tsdfId = Guid.NewGuid();
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns(manifest);

        var result = await Sut().Handle(CommandFor(manifest.HazardousWasteManifestId.Value, tsdfId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("ReceivedAtTsdf");
        await _repo.Received(1).UpdateAsync(manifest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManifestDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing manifest must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenManifestStillGenerated_ReturnsFailureWithoutPropagating()
    {
        var manifest = Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "MAN-2026-000456", "EPA123456789", Guid.NewGuid());
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns(manifest);

        var result = await Sut().Handle(CommandFor(manifest.HazardousWasteManifestId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.RecordTsdfReceiptFailed");
    }
}
