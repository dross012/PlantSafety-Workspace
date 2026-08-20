// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.FeatureEngineering.Commands
//  Layer:          Tests (Unit)
//  File:           PublishFeatureSetCommandHandlerTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PublishFeatureSetCommandHandler. Covers a successful publish,
//                  caller ETag forwarding, tenant-scoped NotFound, the domain publish-guard
//                  exceptions (FeatureSetAlreadyPublishedException, FeatureSetEmptyException) ->
//                  Domain ApplicationError mapping, and ConcurrencyConflictException propagation.
//                  Mirrors
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

public sealed class PublishFeatureSetCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureSetRepository _repo = Substitute.For<IFeatureSetRepository>();

    private PublishFeatureSetCommandHandler Sut() => new(_repo, NullLogger<PublishFeatureSetCommandHandler>());

    private static FeatureSetEntity FeatureSetWith(int featureCount)
    {
        var featureSet = FeatureSetEntity.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, name: "IncidentRiskFeatures",
            version: FeatureVersion.From("1.0"), dataSourcePath: "masked://datalake/incident-risk");

        for (var i = 0; i < featureCount; i++)
            featureSet.AddFeature(FeatureName.From($"feature_{i}"), TestCorrelationId, TestCausationId);

        return featureSet;
    }

    private static PublishFeatureSetCommand CommandFor(FeatureSetEntity featureSet, string? eTag) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        FeatureSetId: featureSet.FeatureSetId.Value,
        PublishedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithAtLeastOneFeature_PublishesAndForwardsEtag()
    {
        var featureSet = FeatureSetWith(2);
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);

        var result = await Sut().Handle(CommandFor(featureSet, "0x01"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPublished.Should().BeTrue();
        result.Value.FeatureCount.Should().Be(2);
        await _repo.Received(1).UpdateAsync(featureSet, "0x01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFeatureSetDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FeatureSetEntity?)null);

        var result = await Sut().Handle(CommandFor(FeatureSetWith(1), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenFeatureSetHasNoFeatures_ReturnsEmptyFeatureSetDomainError()
    {
        var featureSet = FeatureSetWith(0);
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);

        var result = await Sut().Handle(CommandFor(featureSet, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.FeatureEngineering.EmptyFeatureSet");
        result.Error.Severity.Should().Be(ErrorSeverity.Domain);
    }

    [Fact]
    public async Task Handle_WhenFeatureSetAlreadyPublished_ReturnsAlreadyPublishedDomainError()
    {
        var featureSet = FeatureSetWith(1);
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
        var featureSet = FeatureSetWith(1);
        _repo.GetByIdAsync(Arg.Any<FeatureSetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(featureSet);
        _repo.UpdateAsync(Arg.Any<FeatureSetEntity>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(featureSet, "stale"), CancellationToken.None));
    }
}
