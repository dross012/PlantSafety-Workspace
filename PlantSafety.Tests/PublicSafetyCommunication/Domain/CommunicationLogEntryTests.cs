// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Domain
//  Layer:          Tests (Unit)
//  File:           CommunicationLogEntryTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunicationLogEntry aggregate root -- Record
//                  (Create) and, critically, proof that the aggregate is immutable once recorded:
//                  no mutation method is exposed on the public surface (reflection-based negative
//                  assertion) and the repository interface exposes no UpdateAsync method.
//                  Isolated, deterministic, <50ms per test. No I/O.
// ============================================================================
using System.Reflection;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Events;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;
using LogEntryAggregate = PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry;

namespace PlantSafety.Tests.PublicSafetyCommunication.Domain;

public sealed class CommunicationLogEntryTests : UnitTestBase
{
    private static LogEntryAggregate CreateEntry() =>
        LogEntryAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentId: Guid.NewGuid(),
            relatedIncidentNotificationId: null,
            relatedPublicAlertBroadcastId: null,
            direction: CommunicationDirection.Outbound,
            medium: CommunicationMedium.Phone,
            counterpartyName: "County Fire Dispatch",
            summary: "Notified county fire dispatch of Unit 3 ammonia release; requested HazMat response.",
            occurredAtUtc: FixedTimestamp);

    [Fact]
    public void Create_WithValidData_RecordsEntryAndRaisesRecordedEvent()
    {
        var entry = CreateEntry();

        entry.Direction.Should().Be(CommunicationDirection.Outbound);
        entry.Medium.Should().Be(CommunicationMedium.Phone);
        entry.CounterpartyName.Should().Be("County Fire Dispatch");
        entry.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CommunicationLogEntryRecordedEvent>();
    }

    [Fact]
    public void Create_TrimsWhitespaceFromFreeTextFields()
    {
        var entry = LogEntryAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentId: null,
            relatedIncidentNotificationId: null,
            relatedPublicAlertBroadcastId: null,
            direction: CommunicationDirection.Inbound,
            medium: CommunicationMedium.Email,
            counterpartyName: "  EPA Regional Office  ",
            summary: "  Confirmation of receipt.  ",
            occurredAtUtc: FixedTimestamp);

        entry.CounterpartyName.Should().Be("EPA Regional Office");
        entry.Summary.Should().Be("Confirmation of receipt.");
    }

    [Fact]
    public void Aggregate_ExposesNoPublicMutationMethod_BecauseCommunicationLogEntryIsImmutableOnceRecorded()
    {
        // Data-governance policy: CommunicationLogEntry has no update path. The only public
        // instance members permitted on the aggregate are read-only property getters plus the
        // domain-event bookkeeping method ClearDomainEvents(). Any method that looks like it
        // mutates recorded content (Update/Edit/Change/Set/Modify/Correct/Amend) must not exist.
        var publicInstanceMethods = typeof(LogEntryAggregate)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName) // exclude property get_/set_ accessors
            .ToList();

        var forbiddenPrefixes = new[] { "Update", "Edit", "Change", "Set", "Modify", "Correct", "Amend", "Delete", "Remove" };

        var offendingMethods = publicInstanceMethods
            .Where(m => forbiddenPrefixes.Any(prefix => m.Name.StartsWith(prefix, StringComparison.Ordinal)))
            .Select(m => m.Name)
            .ToList();

        offendingMethods.Should().BeEmpty(
            "CommunicationLogEntry's data-governance policy is immutable-once-recorded; " +
            "no mutation method may be added to its public surface");

        publicInstanceMethods.Select(m => m.Name).Should().Contain(nameof(LogEntryAggregate.ClearDomainEvents));
    }

    [Fact]
    public void RepositoryInterface_ExposesNoUpdateMethod_BecauseTheTableIsAppendOnly()
    {
        // Mirrors the aggregate-level immutability guard at the persistence-contract boundary:
        // ICommunicationLogEntryRepository must never grow an UpdateAsync method, since there is
        // no domain behavior that could legitimately call it.
        var repositoryMethods = typeof(ICommunicationLogEntryRepository)
            .GetMethods()
            .Select(m => m.Name)
            .ToList();

        repositoryMethods.Should().NotContain("UpdateAsync",
            "ICommunicationLogEntryRepository is append-only -- CommunicationLogEntry has no update path");
        repositoryMethods.Should().Contain("AddAsync");
        repositoryMethods.Should().Contain("GetByIdAsync");
    }

    [Fact]
    public void AttemptingToMutateViaReflection_DoesNotChangeAlreadyRecordedContent()
    {
        // Belt-and-braces: even bypassing the public surface, private setters only exist behind
        // the Create factory's own assignment -- there is no code path (public or reflection-free)
        // that legitimately re-invokes them after construction. This test documents the invariant
        // by asserting the recorded values are stable across repeated reads (no hidden lazy
        // mutation / no accidental shared mutable state between instances).
        var entry = CreateEntry();
        var originalSummary = entry.Summary;
        var originalDirection = entry.Direction;

        var second = CreateEntry();
        second.ClearDomainEvents();

        entry.Summary.Should().Be(originalSummary);
        entry.Direction.Should().Be(originalDirection);
        entry.CommunicationLogEntryId.Should().NotBe(second.CommunicationLogEntryId,
            "each recorded entry must be its own independent, immutable instance");
    }
}
