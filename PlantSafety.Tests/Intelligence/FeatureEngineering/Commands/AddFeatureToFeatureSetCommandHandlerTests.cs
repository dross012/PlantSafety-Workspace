// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.FeatureEngineering.Commands
//  Layer:          Tests (Unit)
//  File:           AddFeatureToFeatureSetCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AddFeatureToFeatureSetCommandHandler. Covers a successful add,
//                  caller ETag forwarding, tenant-scoped NotFound, the
//                  FeatureSetAlreadyPublishedException -> Domain ApplicationError mapping, and
//                  ConcurrencyConflictException propagation. Mirrors
//                  PlantSafety.Tests.Intelligence.DataLake.Commands.StartProcessingDataLakeActivityCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.FeatureEngineering.Commands;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;
using FeatureSetEntity = PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet;

namespace PlantSafety.Tests.Intelligence.FeatureEngineering.Commands;

public sealed class AddFeatureToFeatureSetCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureSetRepository _repo = Substitute.For<IFeatureSetRepository>();

    private AddFeatureToFeatureSetCommandHandler Sut() => new(_repo, NullLogger<AddFeatureToFeatureSetCommandHandler>());

    private static FeatureSetEntity UnpublishedFeatureSet() =>
        FeatureSetEntity.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, name: "IncidentRiskFeatures",
            version: FeatureVersion.From("1.0"), dataSourcePath: "masked://datalake/incident-risk");

    private static AddFeatureToFeatureSetCommand CommandFor(FeatureSetEntity featureSet, string? eTag, string featureName = "worker_fatigue_score") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        FeatureSetId: featureSet.FeatureSetId.Value,
        FeatureName: featureName,
        RequestedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_AddsFeatureAndForwardsEtag()
    {
        var featureSet = UnpublishedFeatureSet();
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);

        var result = await Sut().Handle(CommandFor(featureSet, "0x01"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FeatureCount.Should().Be(1);
        result.Value.IsPublished.Should().BeFalse();
        await _repo.Received(1).UpdateAsync(featureSet, "0x01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFeatureSetDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FeatureSetEntity?)null);

        var result = await Sut().Handle(CommandFor(UnpublishedFeatureSet(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenFeatureSetAlreadyPublished_ReturnsDomainError()
    {
        var featureSet = UnpublishedFeatureSet();
        featureSet.AddFeature(PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.FeatureName.From("existing"), TestCorrelationId, TestCausationId);
        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);

        var result = await Sut().Handle(CommandFor(featureSet, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.AlreadyPublished");
        result.Error.Severity.Should().Be(ErrorSeverity.Domain);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var featureSet = UnpublishedFeatureSet();
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);
        _repo.UpdateAsync(Arg.Any<FeatureSetEntity>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(featureSet, "stale"), CancellationToken.None));
    }
}
