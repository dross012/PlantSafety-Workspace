// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Aggregates
//  Layer:          Tests (Unit)
//  File:           CommunityConcernTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunityConcern aggregate root (Complaint/
//                  Concern Intake subdomain, and the primary aggregate for CommunitySafety).
//                  Covers Log invariants, BeginReview/Escalate/Resolve/Archive lifecycle
//                  transitions, severity bump-on-escalation, and history-entry accumulation.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Exceptions;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Aggregates;

public sealed class CommunityConcernTests : UnitTestBase
{
    private static Agg LoggedConcern(string severity = "Low") =>
        Agg.Log(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            category: CommunityConcernCategory.Odor,
            severity: severity,
            facilityArea: "Tank Farm",
            description: "Strong odor reported near the fence line.");

    [Fact]
    public void Log_WithValidFields_CreatesOpenConcernAndRaisesEventAndHistoryEntry()
    {
        var concern = LoggedConcern();

        concern.Status.Should().Be(CommunityConcernStatus.Open);
        concern.EscalationCount.Should().Be(0);
        concern.DomainEvents.Should().ContainSingle();
        concern.PendingHistoryEntries.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Log_WithMissingDescription_ThrowsArgumentException(string? description)
    {
        var act = () => Agg.Log(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), CommunityConcernCategory.Noise, "Low", "Tank Farm", description!);

        act.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Log_WithEmptySiteId_ThrowsArgumentException()
    {
        var act = () => Agg.Log(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.Empty, CommunityConcernCategory.Noise, "Low", "Tank Farm", "desc");

        act.Should().Throw<ArgumentException>().WithParameterName("siteId");
    }

    [Fact]
    public void BeginReview_FromOpen_TransitionsToUnderReview()
    {
        var concern = LoggedConcern();

        concern.BeginReview(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        concern.Status.Should().Be(CommunityConcernStatus.UnderReview);
    }

    [Theory]
    [InlineData("Low", "Medium")]
    [InlineData("Medium", "High")]
    [InlineData("High", "Critical")]
    [InlineData("Critical", "Critical")]
    public void Escalate_FromOpen_BumpsSeverityAndRaisesEventAndIncrementsCount(string startSeverity, string expectedSeverity)
    {
        var concern = LoggedConcern(startSeverity);

        concern.Escalate("Third complaint this month for this area.", recentConcernCountForArea: 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        concern.Status.Should().Be(CommunityConcernStatus.Escalated);
        concern.Severity.Should().Be(expectedSeverity);
        concern.EscalationCount.Should().Be(1);
        concern.EscalationReason.Should().Be("Third complaint this month for this area.");
        concern.DomainEvents.Should().HaveCount(2, "Log + Escalate each raise one domain event");
    }

    [Fact]
    public void Escalate_WithEmptyReason_ThrowsArgumentException()
    {
        var concern = LoggedConcern();

        var act = () => concern.Escalate("", 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>().WithParameterName("reason");
    }

    [Fact]
    public void Escalate_WhenResolved_ThrowsInvalidStatusTransitionException()
    {
        var concern = LoggedConcern();
        concern.Resolve("Addressed via process adjustment.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => concern.Escalate("Reason", 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityConcernInvalidStatusTransitionException>();
    }

    [Fact]
    public void Resolve_FromOpen_TransitionsAndRecordsNotesAndTimestamp()
    {
        var concern = LoggedConcern();

        concern.Resolve("Root cause corrected; odor source sealed.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        concern.Status.Should().Be(CommunityConcernStatus.Resolved);
        concern.ResolutionNotes.Should().Be("Root cause corrected; odor source sealed.");
        concern.ResolvedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Resolve_WithEmptyNotes_ThrowsArgumentException()
    {
        var concern = LoggedConcern();

        var act = () => concern.Resolve("   ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>().WithParameterName("resolutionNotes");
    }

    [Fact]
    public void Archive_FromResolved_Transitions()
    {
        var concern = LoggedConcern();
        concern.Resolve("Fixed.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        concern.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        concern.Status.Should().Be(CommunityConcernStatus.Archived);
    }

    [Fact]
    public void Archive_FromOpen_ThrowsInvalidStatusTransitionException()
    {
        var concern = LoggedConcern();

        var act = () => concern.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityConcernInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearPendingHistoryEntries_RemovesAllEntries()
    {
        var concern = LoggedConcern();
        concern.PendingHistoryEntries.Should().NotBeEmpty();

        concern.ClearPendingHistoryEntries();

        concern.PendingHistoryEntries.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_ReconstructsStateWithoutRaisingDomainEvents()
    {
        var original = LoggedConcern();

        var rehydrated = Agg.Rehydrate(
            original.CommunityConcernId, original.TenantId, original.SiteId, original.Status,
            original.Category, original.Severity, original.FacilityArea, original.Description,
            original.ReportedByContactId, original.RelatedIncidentId, original.LoggedAtUtc,
            original.CreatedAtUtc, original.CreatedBy, original.EscalationCount, original.EscalationReason,
            original.EscalatedAtUtc, original.ResolutionNotes, original.ResolvedAtUtc);

        rehydrated.CommunityConcernId.Should().Be(original.CommunityConcernId);
        rehydrated.DomainEvents.Should().BeEmpty("rehydration must never re-raise domain events");
    }
}
