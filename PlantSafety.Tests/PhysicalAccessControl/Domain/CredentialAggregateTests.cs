// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           CredentialAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Credential aggregate root: full badge lifecycle
//                  (Issue -> Suspend -> Revoke / Expire), invariant-guard tests for illegal
//                  transitions, and domain-event assertions. Credential/PII fields (BadgeNumber,
//                  HolderUserId) are exercised but never logged/asserted in plaintext beyond what
//                  the aggregate itself exposes for internal invariants.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="Credential"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class CredentialAggregateTests : UnitTestBase
{
    private static Credential CreateIssued(
        CredentialHolderType holderType = CredentialHolderType.Employee,
        UtcTimestamp? expiresAtUtc = null) =>
        Credential.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            badgeNumber: "BADGE-12345",
            holderUserId: Guid.NewGuid(),
            holderType: holderType,
            accessLevel: "General",
            expiresAtUtc: expiresAtUtc);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateIssued().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusIssued()
    {
        var agg = CreateIssued();
        agg.Status.Should().Be(CredentialStatus.Issued);
    }

    [Fact]
    public void Create_ShouldTrimBadgeNumberAndAccessLevel()
    {
        var agg = Credential.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            badgeNumber: "  BADGE-999  ",
            holderUserId: Guid.NewGuid(),
            accessLevel: "  RestrictedZoneA  ");

        agg.BadgeNumber.Should().Be("BADGE-999");
        agg.AccessLevel.Should().Be("RestrictedZoneA");
    }

    [Fact]
    public void Create_ShouldRaiseCredentialIssuedEvent()
    {
        var agg = CreateIssued();
        agg.DomainEvents.ShouldRaiseSingleEvent<CredentialIssuedEvent>();
    }

    [Theory]
    [InlineData(CredentialHolderType.Employee)]
    [InlineData(CredentialHolderType.Contractor)]
    [InlineData(CredentialHolderType.Visitor)]
    public void Create_WithHolderType_ShouldPersistClassification(CredentialHolderType holderType)
    {
        var agg = CreateIssued(holderType);
        agg.HolderType.Should().Be(holderType);
    }

    [Fact]
    public void Create_WithExpiresAtUtc_ShouldPersistExpiry()
    {
        var expiry = UtcTimestamp.From(FixedUtcNow.AddYears(1));
        var agg = CreateIssued(expiresAtUtc: expiry);
        agg.ExpiresAtUtc.Should().Be(expiry);
    }

    [Fact]
    public void Create_WithoutExpiresAtUtc_ShouldHaveNullExpiry()
    {
        var agg = CreateIssued();
        agg.ExpiresAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateIssued();
        var a2 = CreateIssued();
        a1.CredentialId.Should().NotBe(a2.CredentialId);
    }

    // ---- Suspend -----------------------------------------------------------------

    [Fact]
    public void Suspend_WhenIssued_ShouldTransitionToSuspended()
    {
        var agg = CreateIssued();
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CredentialStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenIssued_ShouldRaiseEvent()
    {
        var agg = CreateIssued();
        agg.ClearDomainEvents();
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CredentialSuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIssued();
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Revoke ------------------------------------------------------------------

    [Fact]
    public void Revoke_WhenIssued_ShouldTransitionToRevoked()
    {
        var agg = CreateIssued();
        agg.Revoke("Lost badge", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CredentialStatus.Revoked);
    }

    [Fact]
    public void Revoke_ShouldTrimAndPersistReason()
    {
        var agg = CreateIssued();
        agg.Revoke("  Lost badge  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RevocationReason.Should().Be("Lost badge");
    }

    [Fact]
    public void Revoke_FromSuspended_ShouldTransitionToRevoked()
    {
        var agg = CreateIssued();
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Revoke("Terminated employment", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CredentialStatus.Revoked);
    }

    [Fact]
    public void Revoke_ShouldRaiseEvent()
    {
        var agg = CreateIssued();
        agg.ClearDomainEvents();
        agg.Revoke("Lost badge", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CredentialRevokedEvent>();
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIssued();
        agg.Revoke("Lost badge", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Revoke("Second reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Revoke_WhenExpired_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIssued();
        agg.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Revoke("Too late", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Expire ------------------------------------------------------------------

    [Fact]
    public void Expire_WhenIssued_ShouldTransitionToExpired()
    {
        var agg = CreateIssued();
        agg.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CredentialStatus.Expired);
    }

    [Fact]
    public void Expire_FromSuspended_ShouldTransitionToExpired()
    {
        var agg = CreateIssued();
        agg.Suspend(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CredentialStatus.Expired);
    }

    [Fact]
    public void Expire_ShouldRaiseEvent()
    {
        var agg = CreateIssued();
        agg.ClearDomainEvents();
        agg.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CredentialExpiredEvent>();
    }

    [Fact]
    public void Expire_WhenAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIssued();
        agg.Revoke("Lost badge", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Expire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}
