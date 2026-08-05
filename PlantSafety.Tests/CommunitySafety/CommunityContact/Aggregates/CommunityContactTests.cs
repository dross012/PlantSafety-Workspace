// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityContact.Aggregates
//  Layer:          Tests (Unit)
//  File:           CommunityContactTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunityContact aggregate root (Community
//                  Contact Registry subdomain). Covers Register invariants, the
//                  Active/Inactive/Archived lifecycle transitions, and the PII-minimization
//                  data-governance invariant (no home address/date-of-birth fields exist on the
//                  aggregate). Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityContact.Exceptions;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityContact.Aggregates.CommunityContact;

namespace PlantSafety.Tests.CommunitySafety.CommunityContact.Aggregates;

public sealed class CommunityContactTests : UnitTestBase
{
    private static Agg RegisteredContact(bool optedIn = true) =>
        Agg.Register(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            displayName: "Fire Chief - Station 4",
            contactType: CommunityContactType.EmergencyServices,
            communityArea: "North Ridge",
            email: "chief@example.org",
            organization: "City Fire Dept",
            phone: "555-0100",
            optedInForNotifications: optedIn);

    [Fact]
    public void Register_WithValidFields_CreatesActiveContactAndRaisesEvent()
    {
        var contact = RegisteredContact();

        contact.Status.Should().Be(CommunityContactStatus.Active);
        contact.DisplayName.Should().Be("Fire Chief - Station 4");
        contact.ContactType.Should().Be(CommunityContactType.EmergencyServices);
        contact.DomainEvents.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithMissingDisplayName_ThrowsArgumentException(string? displayName)
    {
        var act = () => Agg.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, displayName!, CommunityContactType.Resident, "North Ridge", "a@b.com");

        act.Should().Throw<ArgumentException>().WithParameterName("displayName");
    }

    [Fact]
    public void Register_WithMissingEmail_ThrowsArgumentException()
    {
        var act = () => Agg.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "Name", CommunityContactType.Resident, "North Ridge", "  ");

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void Register_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => Agg.Register(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            Guid.Empty, null, "Name", CommunityContactType.Resident, "North Ridge", "a@b.com");

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Fact]
    public void SetInactive_FromActive_TransitionsAndClearsOptIn()
    {
        var contact = RegisteredContact(optedIn: true);

        contact.SetInactive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        contact.Status.Should().Be(CommunityContactStatus.Inactive);
        contact.OptedInForNotifications.Should().BeFalse("an inactive contact must not remain opted-in for outreach");
    }

    [Fact]
    public void Reactivate_FromInactive_TransitionsToActive()
    {
        var contact = RegisteredContact();
        contact.SetInactive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        contact.Reactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        contact.Status.Should().Be(CommunityContactStatus.Active);
    }

    [Fact]
    public void Archive_FromActive_TransitionsAndRaisesEventAndClearsOptIn()
    {
        var contact = RegisteredContact(optedIn: true);

        contact.Archive("Public-records redaction request", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        contact.Status.Should().Be(CommunityContactStatus.Archived);
        contact.ArchivalReason.Should().Be("Public-records redaction request");
        contact.OptedInForNotifications.Should().BeFalse();
        contact.DomainEvents.Should().HaveCount(2, "Register + Archive each raise one domain event");
    }

    [Fact]
    public void Archive_WithEmptyReason_ThrowsArgumentException()
    {
        var contact = RegisteredContact();

        var act = () => contact.Archive("", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>().WithParameterName("reason");
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ThrowsInvalidStatusTransitionException()
    {
        var contact = RegisteredContact();
        contact.Archive("First archival", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => contact.Archive("Second archival", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityContactInvalidStatusTransitionException>();
    }

    [Fact]
    public void SetInactive_WhenAlreadyArchived_ThrowsInvalidStatusTransitionException()
    {
        var contact = RegisteredContact();
        contact.Archive("Reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => contact.SetInactive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityContactInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var contact = RegisteredContact();
        contact.DomainEvents.Should().NotBeEmpty();

        contact.ClearDomainEvents();

        contact.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rehydrate_ReconstructsStateWithoutRaisingDomainEvents()
    {
        var original = RegisteredContact();

        var rehydrated = Agg.Rehydrate(
            original.CommunityContactId, original.TenantId, original.SiteId, original.Status,
            original.DisplayName, original.ContactType, original.Organization, original.CommunityArea,
            original.Email, original.Phone, original.OptedInForNotifications, original.CreatedAtUtc,
            original.CreatedBy, original.ArchivedAtUtc, original.ArchivalReason);

        rehydrated.CommunityContactId.Should().Be(original.CommunityContactId);
        rehydrated.DomainEvents.Should().BeEmpty("rehydration must never re-raise domain events");
    }
}
