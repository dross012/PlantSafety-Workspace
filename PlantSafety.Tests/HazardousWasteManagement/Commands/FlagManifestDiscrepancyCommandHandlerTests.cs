// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           FlagManifestDiscrepancyCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for FlagManifestDiscrepancyCommandHandler, including the
//                  ErrorSeverity.NotFound regression this test suite surfaced and fixed.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class FlagManifestDiscrepancyCommandHandlerTests : UnitTestBase
{
    private readonly IHazardousWasteManifestRepository _repo = Substitute.For<IHazardousWasteManifestRepository>();

    private FlagManifestDiscrepancyCommandHandler Sut() => new(_repo, NullLogger<FlagManifestDiscrepancyCommandHandler>());

    private static Agg GeneratedManifest() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "MAN-2026-000123", "EPA123456789", Guid.NewGuid());

    private static FlagManifestDiscrepancyCommand CommandFor(Guid manifestId, string reason) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        HazardousWasteManifestId: manifestId,
        DiscrepancyReason: reason,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithExistingManifest_FlagsDiscrepancyAndReturnsSuccess()
    {
        var manifest = GeneratedManifest();
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns(manifest);

        var result = await Sut().Handle(CommandFor(manifest.HazardousWasteManifestId.Value, "Quantity mismatch"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("DiscrepancyFlagged");
        manifest.DiscrepancyReason.Should().Be("Quantity mismatch");
        await _repo.Received(1).UpdateAsync(manifest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManifestDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid(), "Reason"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing manifest must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WithEmptyDiscrepancyReason_ReturnsFailureWithoutPropagating()
    {
        var manifest = GeneratedManifest();
        _repo.GetByIdAsync(Arg.Any<HazardousWasteManifestId>(), Arg.Any<CancellationToken>()).Returns(manifest);

        var result = await Sut().Handle(CommandFor(manifest.HazardousWasteManifestId.Value, "   "), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.FlagDiscrepancyFailed");
    }
}
