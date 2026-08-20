// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.FeatureEngineering
//  Layer:          Tests (Unit)
//  File:           FeatureSetTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FeatureSet entity: Create factory guards, AddFeature
//                  mutation-before-publish rule, Publish immutability/empty guards, and domain
//                  event emission.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.Entities;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Events;
using PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.FeatureEngineering;

/// <summary>Unit tests for the <see cref="FeatureSet"/> entity.</summary>
public sealed class FeatureSetTests : UnitTestBase
{
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

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_WithValidData_StartsUnpublished()
    {
        var featureSet = NewFeatureSet();
        featureSet.IsPublished.Should().BeFalse();
        featureSet.Features.Should().BeEmpty();
    }

    [Fact]
    public void Create_RaisesFeatureSetCreatedEvent()
    {
        var featureSet = NewFeatureSet();
        featureSet.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<FeatureSetCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => FeatureSet.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, name: "N", version: FeatureVersion.From("1.0"), dataSourcePath: "path");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => FeatureSet.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, name: "   ", version: FeatureVersion.From("1.0"), dataSourcePath: "path");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyDataSourcePath_ThrowsArgumentException()
    {
        var act = () => FeatureSet.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, name: "N", version: FeatureVersion.From("1.0"), dataSourcePath: "  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullVersion_ThrowsArgumentNullException()
    {
        var act = () => FeatureSet.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: null, name: "N", version: null!, dataSourcePath: "path");
        act.Should().Throw<ArgumentNullException>();
    }

    // ---- AddFeature --------------------------------------------------------------

    [Fact]
    public void AddFeature_BeforePublish_AddsFeature()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);

        featureSet.Features.Should().ContainSingle(f => f.Value == "worker_fatigue_score");
    }

    [Fact]
    public void AddFeature_Duplicate_DoesNotAddTwice()
    {
        var featureSet = NewFeatureSet();
        var feature = FeatureName.From("worker_fatigue_score");
        featureSet.AddFeature(feature, TestCorrelationId, TestCausationId);
        featureSet.AddFeature(feature, TestCorrelationId, TestCausationId);

        featureSet.Features.Should().HaveCount(1);
    }

    [Fact]
    public void AddFeature_AfterPublish_ThrowsFeatureSetAlreadyPublished()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);
        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => featureSet.AddFeature(FeatureName.From("anomaly_score"), TestCorrelationId, TestCausationId);
        act.Should().Throw<FeatureSetAlreadyPublishedException>();
    }

    [Fact]
    public void AddFeature_WithNullFeatureName_ThrowsArgumentNull()
    {
        var featureSet = NewFeatureSet();
        var act = () => featureSet.AddFeature(null!, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---- Publish -------------------------------------------------------------------

    [Fact]
    public void Publish_WithAtLeastOneFeature_MarksPublished()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);

        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        featureSet.IsPublished.Should().BeTrue();
        featureSet.PublishedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Publish_RaisesFeatureSetPublishedEvent()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);
        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        featureSet.DomainEvents.Should().Contain(e => e is FeatureSetPublishedEvent);
    }

    [Fact]
    public void Publish_WithNoFeatures_ThrowsFeatureSetEmpty()
    {
        var featureSet = NewFeatureSet();
        var act = () => featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<FeatureSetEmptyException>();
    }

    [Fact]
    public void Publish_AlreadyPublished_ThrowsFeatureSetAlreadyPublished()
    {
        var featureSet = NewFeatureSet();
        featureSet.AddFeature(FeatureName.From("worker_fatigue_score"), TestCorrelationId, TestCausationId);
        featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => featureSet.Publish(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<FeatureSetAlreadyPublishedException>(
            "a published FeatureSet is immutable and can never be re-published");
    }

    // ---- Domain events -------------------------------------------------------------

    [Fact]
    public void ClearDomainEvents_EmptiesDomainEventsCollection()
    {
        var featureSet = NewFeatureSet();
        featureSet.DomainEvents.Should().NotBeEmpty();

        featureSet.ClearDomainEvents();

        featureSet.DomainEvents.Should().BeEmpty();
    }
}
