// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.FeatureEngineering
//  Layer:          Tests (Unit)
//  File:           FeatureEngineeringDomainServiceTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for FeatureEngineeringDomainService: feature-set versioning
//                  (Supersedes) and publish-readiness (IsReadyToPublish) business rules. Mirrors
//                  PlantSafety.Tests.Domain.Intelligence.PredictiveSafety.FailureLikelihoodPredictorTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.Entities;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Services;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.FeatureEngineering;

/// <summary>Unit tests for <see cref="FeatureEngineeringDomainService"/>.</summary>
public sealed class FeatureEngineeringDomainServiceTests : UnitTestBase
{
    private readonly FeatureEngineeringDomainService _sut = new();

    private static FeatureSet NewFeatureSet(string version = "1.0", string name = "IncidentRiskFeatures") =>
        FeatureSet.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            name: name,
            version: FeatureVersion.From(version),
            dataSourcePath: "masked://datalake/incident-risk");

    // ---- Supersedes ----------------------------------------------------------

    [Theory]
    [InlineData("2.0", "1.0", true)]
    [InlineData("1.1", "1.0", true)]
    [InlineData("1.0", "1.0", false)]
    [InlineData("1.0", "1.1", false)]
    [InlineData("1.0", "2.0", false)]
    public void Supersedes_ComparesOrdinally(string candidate, string existing, bool expected)
    {
        _sut.Supersedes(FeatureVersion.From(candidate), FeatureVersion.From(existing)).Should().Be(expected);
    }

    [Fact]
    public void Supersedes_WithNullCandidate_ThrowsArgumentNull()
    {
        var act = () => _sut.Supersedes(null!, FeatureVersion.From("1.0"));
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Supersedes_WithNullExisting_ThrowsArgumentNull()
    {
        var act = () => _sut.Supersedes(FeatureVersion.From("1.0"), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---- IsReadyToPublish ------------------------------------------------------

    [Fact]
    public void IsReadyToPublish_UnpublishedWithFeatures_ReturnsTrue()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);

        _sut.IsReadyToPublish(featureSet).Should().BeTrue();
    }

    [Fact]
    public void IsReadyToPublish_UnpublishedWithNoFeatures_ReturnsFalse()
    {
        var featureSet = NewFeatureSet();
        _sut.IsReadyToPublish(featureSet).Should().BeFalse();
    }

    [Fact]
    public void IsReadyToPublish_AlreadyPublished_ReturnsFalse()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);
        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        _sut.IsReadyToPublish(featureSet).Should().BeFalse("an already-published feature set is immutable and cannot be re-published");
    }

    [Fact]
    public void IsReadyToPublish_WithNullFeatureSet_ThrowsArgumentNull()
    {
        var act = () => _sut.IsReadyToPublish(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
