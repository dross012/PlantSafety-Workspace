// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Domain
//  Layer:          Tests (Unit)
//  File:           PublicSafetyAgencyTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the PublicSafetyAgency aggregate root -- Register
//                  (Create), AddContact, RecordVerification, MarkVerificationOverdue, Deactivate,
//                  and their guard rules. Isolated, deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Events;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Exceptions;
using AgencyAggregate = PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency;

namespace PlantSafety.Tests.PublicSafetyCommunication.Domain;

public sealed class PublicSafetyAgencyTests : UnitTestBase
{
    private static AgencyAggregate CreateAgency() =>
        AgencyAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            agencyName: "County Fire Department",
            agencyType: PublicSafetyAgencyType.Fire,
            jurisdictionDescription: "Unincorporated county district 4",
            primaryPhoneNumber: "555-0100",
            primaryEmail: "dispatch@countyfire.example");

    [Fact]
    public void Create_WithValidData_InitializesActiveUnverifiedAgencyAndRaisesRegisteredEvent()
    {
        var agency = CreateAgency();

        agency.Status.Should().Be(AgencyOperationalStatus.Active);
        agency.VerificationStatus.Should().Be(AgencyVerificationStatus.Unverified);
        agency.AgencyName.Should().Be("County Fire Department");
        agency.Contacts.Should().BeEmpty();
        agency.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PublicSafetyAgencyRegisteredEvent>();
    }

    [Fact]
    public void Create_TrimsWhitespaceFromFreeTextFields()
    {
        var agency = AgencyAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            agencyName: "  Metro Police  ",
            jurisdictionDescription: "  Downtown  ",
            primaryPhoneNumber: "  555-0111  ");

        agency.AgencyName.Should().Be("Metro Police");
        agency.JurisdictionDescription.Should().Be("Downtown");
        agency.PrimaryPhoneNumber.Should().Be("555-0111");
    }

    [Fact]
    public void AddContact_AppendsContactAndRaisesEvent()
    {
        var agency = CreateAgency();
        agency.ClearDomainEvents();

        var contact = agency.AddContact(
            AgencyContactRole.PrimaryDispatch,
            "County Dispatch Center",
            "555-0199",
            "dispatch2@countyfire.example",
            FixedTimestamp,
            TestActor,
            TestCorrelationId,
            TestCausationId);

        agency.Contacts.Should().ContainSingle();
        agency.Contacts.Single().Should().BeSameAs(contact);
        agency.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AgencyContactAddedEvent>();
    }

    [Fact]
    public void RecordVerification_WhenActive_UpdatesVerificationStateAndRaisesEvent()
    {
        var agency = CreateAgency();
        agency.ClearDomainEvents();
        var verifiedAt = FixedTimestamp;
        var nextDue = UtcTimestamp.From(FixedUtcNow.AddMonths(6));

        agency.RecordVerification(verifiedAt, nextDue, TestActor, TestCorrelationId, TestCausationId);

        agency.VerificationStatus.Should().Be(AgencyVerificationStatus.Verified);
        agency.LastVerifiedAtUtc.Should().Be(verifiedAt);
        agency.NextVerificationDueAtUtc.Should().Be(nextDue);
        agency.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AgencyVerificationRecordedEvent>();
    }

    [Fact]
    public void RecordVerification_WhenInactive_ThrowsInvalidStatusTransition()
    {
        var agency = CreateAgency();
        agency.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agency.RecordVerification(FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PublicSafetyAgencyInvalidStatusTransitionException>();
    }

    [Fact]
    public void MarkVerificationOverdue_SetsStatusAndRaisesEvent()
    {
        var agency = CreateAgency();
        agency.ClearDomainEvents();

        agency.MarkVerificationOverdue(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agency.VerificationStatus.Should().Be(AgencyVerificationStatus.VerificationOverdue);
        agency.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AgencyVerificationOverdueEvent>();
    }

    [Fact]
    public void Deactivate_WhenActive_TransitionsToInactiveAndRaisesEvent()
    {
        var agency = CreateAgency();
        agency.ClearDomainEvents();

        agency.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agency.Status.Should().Be(AgencyOperationalStatus.Inactive);
        agency.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AgencyDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsInvalidStatusTransition()
    {
        var agency = CreateAgency();
        agency.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agency.Deactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PublicSafetyAgencyInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var agency = CreateAgency();
        agency.DomainEvents.Should().NotBeEmpty();

        agency.ClearDomainEvents();

        agency.DomainEvents.Should().BeEmpty();
    }
}
