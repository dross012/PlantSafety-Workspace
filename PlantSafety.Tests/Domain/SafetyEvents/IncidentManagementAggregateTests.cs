// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.SafetyEvents
//  Layer:          Tests (Unit)
//  File:           IncidentManagementAggregateTests.cs
//  Created:        2026-06-17
//  Updated:        2026-07-27 -- Replaced the tests that exercised the old anemic, disconnected
//                  scaffold (parameterless Create(), generic Created/Active/Completed/Closed/
//                  Archived states) with tests for the redesigned aggregate: Create invariants
//                  grounded in [SafetyEvents].[Incident] NOT NULL columns, the real
//                  Reported -> Archived lifecycle (UpdateDetails/Archive), illegal-transition
//                  guards, and Created/Updated/Archived domain event emission.
//  Author:         David (Ted) + Copilot / PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the IncidentManagement aggregate root: creation invariants,
//                  lifecycle transitions, illegal-transition guards, and domain event emission.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Exceptions;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.SafetyEvents;

/// <summary>
/// Unit tests for the <see cref="IncidentManagement"/> aggregate root.
/// Validates creation invariants, lifecycle transitions, illegal-transition guards, and domain
/// event emission.
/// </summary>
public sealed class IncidentManagementAggregateTests : UnitTestBase
{
    private static IncidentManagement CreateReported() =>
        IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            title: "Chemical spill",
            description: "Small chemical spill in Lab 2",
            severity: "Moderate",
            location: "Lab 2",
            reportedByWorkerId: TestUserId);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_ShouldReturnAggregate() =>
        CreateReported().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusReported()
    {
        var agg = CreateReported();
        agg.Status.Should().Be(IncidentManagementStatus.Reported);
    }

    [Fact]
    public void Create_ShouldRaiseIncidentManagementCreatedEvent()
    {
        var agg = CreateReported();
        agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = CreateReported();
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = CreateReported();
        agg.IncidentManagementId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldTrimWhitespaceFromTextFields()
    {
        var agg = IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            title: "  Chemical spill  ", description: "  desc  ", severity: "  Moderate  ",
            location: "  Lab 2  ", reportedByWorkerId: TestUserId);

        agg.Title.Should().Be("Chemical spill");
        agg.Description.Should().Be("desc");
        agg.Severity.Should().Be("Moderate");
        agg.Location.Should().Be("Lab 2");
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateReported();
        var a2 = CreateReported();
        a1.IncidentManagementId.Should().NotBe(a2.IncidentManagementId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException(string title)
    {
        var act = () => IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            title: title, description: "desc", severity: "Moderate", location: "Lab 2",
            reportedByWorkerId: TestUserId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        var act = () => IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: Guid.NewGuid(),
            title: "Title", description: "desc", severity: "Moderate", location: "Lab 2",
            reportedByWorkerId: TestUserId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyReportedByWorkerId_ShouldThrowArgumentException()
    {
        var act = () => IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            title: "Title", description: "desc", severity: "Moderate", location: "Lab 2",
            reportedByWorkerId: default);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyEventList()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- UpdateDetails -----------------------------------------------------------

    [Fact]
    public void UpdateDetails_ShouldChangeOnlyProvidedFields()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();

        agg.UpdateDetails(
            title: "Updated title", description: null, severity: null, location: null,
            assignedInvestigatorId: null, nowUtc: FixedTimestamp, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        agg.Title.Should().Be("Updated title");
        agg.Description.Should().Be("Small chemical spill in Lab 2");
    }

    [Fact]
    public void UpdateDetails_ShouldAssignInvestigator()
    {
        var agg = CreateReported();
        var investigatorId = UserId.New();

        agg.UpdateDetails(
            title: null, description: null, severity: null, location: null,
            assignedInvestigatorId: investigatorId, nowUtc: FixedTimestamp, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        agg.AssignedInvestigatorId.Should().Be(investigatorId);
    }

    [Fact]
    public void UpdateDetails_ShouldRaiseIncidentManagementUpdatedEvent()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();

        agg.UpdateDetails(
            title: "Updated", description: null, severity: null, location: null,
            assignedInvestigatorId: null, nowUtc: FixedTimestamp, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementUpdatedEvent>();
    }

    [Fact]
    public void UpdateDetails_WhenArchived_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        agg.Archive("No longer relevant", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.UpdateDetails(
            title: "New title", description: null, severity: null, location: null,
            assignedInvestigatorId: null, nowUtc: FixedTimestamp, actor: TestActor,
            correlationId: TestCorrelationId, causationId: TestCausationId);

        act.Should().Throw<IncidentManagementInvalidStatusTransitionException>();
    }

    // ---- Archive -----------------------------------------------------------------

    [Fact]
    public void Archive_ShouldTransitionToArchived()
    {
        var agg = CreateReported();
        agg.Archive("Duplicate report", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(IncidentManagementStatus.Archived);
    }

    [Fact]
    public void Archive_ShouldSetDeletionReasonAndArchivedAtUtc()
    {
        var agg = CreateReported();
        agg.Archive("Duplicate report", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DeletionReason.Should().Be("Duplicate report");
        agg.ArchivedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Archive_ShouldRaiseIncidentManagementArchivedEvent()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();

        agg.Archive("Duplicate report", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementArchivedEvent>();
    }

    [Fact]
    public void Archive_WithEmptyReason_ShouldThrowArgumentException()
    {
        var agg = CreateReported();
        var act = () => agg.Archive("", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        agg.Archive("Duplicate report", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Archive("Second reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<IncidentManagementInvalidStatusTransitionException>();
    }

    // ---- Rehydrate ---------------------------------------------------------------

    [Fact]
    public void Rehydrate_ShouldRaiseNoDomainEvents()
    {
        var agg = IncidentManagement.Rehydrate(
            IncidentManagementId.New(), TestTenantId.Value, Guid.NewGuid(), IncidentManagementStatus.Reported,
            "Title", "Description", "Moderate", "Location", FixedTimestamp, FixedTimestamp,
            TestUserId, null, null, null, FixedTimestamp, TestUserId);

        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_ThenArchive_ShouldRaiseArchivedEvent()
    {
        var agg = IncidentManagement.Rehydrate(
            IncidentManagementId.New(), TestTenantId.Value, Guid.NewGuid(), IncidentManagementStatus.Reported,
            "Title", "Description", "Moderate", "Location", FixedTimestamp, FixedTimestamp,
            TestUserId, null, null, null, FixedTimestamp, TestUserId);

        agg.Archive("Reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<IncidentManagementArchivedEvent>();
    }
}
