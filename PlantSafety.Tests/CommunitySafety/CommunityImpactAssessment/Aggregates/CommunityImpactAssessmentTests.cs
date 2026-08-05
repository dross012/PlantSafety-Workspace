// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityImpactAssessment.Aggregates
//  Layer:          Tests (Unit)
//  File:           CommunityImpactAssessmentTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunityImpactAssessment aggregate root.
//                  Covers Record invariants (including the RequiresRightToKnowReporting
//                  Severe-severity Compliance/EPCRA hook) and the Finalize/Archive lifecycle.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Exceptions;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Aggregates.CommunityImpactAssessment;

namespace PlantSafety.Tests.CommunitySafety.CommunityImpactAssessment.Aggregates;

public sealed class CommunityImpactAssessmentTests : UnitTestBase
{
    private static Agg RecordedAssessment(CommunityImpactSeverity severity = CommunityImpactSeverity.Moderate) =>
        Agg.Record(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            incidentId: Guid.NewGuid(),
            severity: severity,
            affectedArea: "North Ridge",
            description: "Vapor cloud drifted toward residential area for ~10 minutes.",
            assessedByUserId: TestUserId,
            estimatedPersonsAffected: 25);

    [Fact]
    public void Record_WithValidFields_CreatesRecordedAssessmentAndRaisesEvent()
    {
        var assessment = RecordedAssessment();

        assessment.Status.Should().Be(CommunityImpactAssessmentStatus.Recorded);
        assessment.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Record_WithSevereSeverity_SetsRequiresRightToKnowReportingTrue()
    {
        var assessment = RecordedAssessment(CommunityImpactSeverity.Severe);

        assessment.RequiresRightToKnowReporting.Should().BeTrue(
            "a Severe community impact must trigger the EPCRA/right-to-know Compliance reporting hook");
    }

    [Theory]
    [InlineData(CommunityImpactSeverity.None)]
    [InlineData(CommunityImpactSeverity.Minor)]
    [InlineData(CommunityImpactSeverity.Moderate)]
    public void Record_WithNonSevereSeverity_SetsRequiresRightToKnowReportingFalse(CommunityImpactSeverity severity)
    {
        var assessment = RecordedAssessment(severity);

        assessment.RequiresRightToKnowReporting.Should().BeFalse();
    }

    [Fact]
    public void Record_WithEmptyIncidentId_ThrowsArgumentException()
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), Guid.Empty, CommunityImpactSeverity.Minor,
            "North Ridge", "desc", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("incidentId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Record_WithMissingAffectedArea_ThrowsArgumentException(string? affectedArea)
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(), CommunityImpactSeverity.Minor,
            affectedArea!, "desc", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("affectedArea");
    }

    [Fact]
    public void Finalize_FromRecorded_TransitionsAndSetsTimestamp()
    {
        var assessment = RecordedAssessment();

        assessment.Finalize(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        assessment.Status.Should().Be(CommunityImpactAssessmentStatus.Finalized);
        assessment.FinalizedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Archive_FromFinalized_Transitions()
    {
        var assessment = RecordedAssessment();
        assessment.Finalize(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        assessment.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        assessment.Status.Should().Be(CommunityImpactAssessmentStatus.Archived);
    }

    [Fact]
    public void Archive_FromRecorded_ThrowsInvalidStatusTransitionException()
    {
        var assessment = RecordedAssessment();

        var act = () => assessment.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityImpactAssessmentInvalidStatusTransitionException>();
    }

    [Fact]
    public void Finalize_WhenAlreadyFinalized_ThrowsInvalidStatusTransitionException()
    {
        var assessment = RecordedAssessment();
        assessment.Finalize(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => assessment.Finalize(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityImpactAssessmentInvalidStatusTransitionException>();
    }

    [Fact]
    public void Rehydrate_ReconstructsStateWithoutRaisingDomainEvents()
    {
        var original = RecordedAssessment();

        var rehydrated = Agg.Rehydrate(
            original.CommunityImpactAssessmentId, original.TenantId, original.SiteId, original.Status,
            original.IncidentId, original.Severity, original.AffectedArea, original.EstimatedPersonsAffected,
            original.Description, original.RequiresRightToKnowReporting, original.AssessedByUserId,
            original.AssessedAtUtc, original.CreatedAtUtc, original.CreatedBy, original.FinalizedAtUtc);

        rehydrated.CommunityImpactAssessmentId.Should().Be(original.CommunityImpactAssessmentId);
        rehydrated.DomainEvents.Should().BeEmpty("rehydration must never re-raise domain events");
    }
}
