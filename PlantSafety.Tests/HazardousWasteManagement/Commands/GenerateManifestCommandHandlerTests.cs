// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           GenerateManifestCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for GenerateManifestCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class GenerateManifestCommandHandlerTests : UnitTestBase
{
    private readonly IHazardousWasteManifestRepository _repo = Substitute.For<IHazardousWasteManifestRepository>();

    private GenerateManifestCommandHandler Sut() => new(_repo, NullLogger<GenerateManifestCommandHandler>());

    private static GenerateManifestCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        ManifestNumber: "MAN-2026-000123",
        GeneratorEpaId: "EPA123456789",
        WasteStreamId: Guid.NewGuid(),
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Generated");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetsThreeYearRetentionOnTheGeneratedManifest()
    {
        // NOTE: the handler stamps timestamps via UtcTimestamp.Now() (real DateTime.UtcNow),
        // not the frozen UnitTestBase clock, so assert the retention delta rather than an
        // exact instant.
        await Sut().Handle(ValidCommand(), CancellationToken.None);

        await _repo.Received(1).AddAsync(
            Arg.Is<Agg>(a => a.RetentionExpiresAtUtc.Value.Year == a.GeneratedAtUtc.Value.AddYears(Agg.MinimumRetentionYears).Year
                          && a.RetentionExpiresAtUtc.Value.Date == a.GeneratedAtUtc.Value.AddYears(Agg.MinimumRetentionYears).Date),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.Manifest.GenerateFailed");
    }
}
