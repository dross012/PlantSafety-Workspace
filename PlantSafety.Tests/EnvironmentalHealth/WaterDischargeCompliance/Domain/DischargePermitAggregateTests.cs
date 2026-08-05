// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           DischargePermitAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the DischargePermit aggregate root
//                  (EnvironmentalHealth.WaterDischargeCompliance, 6-entity sub-module).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Exceptions;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class DischargePermitAggregateTests : UnitTestBase
{
    private static Agg IssuedPermit() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        permitNumber: "NPDES-2026-0042",
        regulatoryProgram: RegulatoryProgramType.Npdes,
        issuingAuthority: "State EPA",
        effectiveAtUtc: FixedTimestamp,
        expiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)),
        permitConditionSummary: "Standard NPDES conditions apply.");

    [Fact]
    public void Create_WithValidData_IssuesActivePermitAndRaisesEvent()
    {
        var permit = IssuedPermit();

        permit.Status.Should().Be(DischargePermitStatus.Active);
        permit.PermitNumber.Should().Be("NPDES-2026-0042");
        permit.RenewalCount.Should().Be(0);
        permit.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_TrimsPermitNumberAndIssuingAuthority()
    {
        var permit = Agg.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
            permitNumber: "  NPDES-9  ", issuingAuthority: "  State EPA  ",
            effectiveAtUtc: FixedTimestamp, expiresAtUtc: UtcTimestamp.From(FixedUtcNow.AddDays(1)));

        permit.PermitNumber.Should().Be("NPDES-9");
        permit.IssuingAuthority.Should().Be("State EPA");
    }

    [Fact]
    public void Renew_WhenActive_AdvancesExpiryAndIncrementsRenewalCount()
    {
        var permit = IssuedPermit();
        var newExpiry = UtcTimestamp.From(FixedUtcNow.AddYears(2));

        permit.Renew(newExpiry, TestActor, TestCorrelationId, TestCausationId);

        permit.ExpiresAtUtc.Should().Be(newExpiry);
        permit.RenewalCount.Should().Be(1);
        permit.Status.Should().Be(DischargePermitStatus.Active);
    }

    [Fact]
    public void FlagExpiringSoon_WhenActive_TransitionsToExpiringSoon()
    {
        var permit = IssuedPermit();

        permit.FlagExpiringSoon(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        permit.Status.Should().Be(DischargePermitStatus.ExpiringSoon);
    }

    [Fact]
    public void FlagExpiringSoon_WhenAlreadyExpiringSoon_ThrowsInvalidStatusTransition()
    {
        var permit = IssuedPermit();
        permit.FlagExpiringSoon(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => permit.FlagExpiringSoon(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePermitInvalidStatusTransitionException>();
    }

    [Fact]
    public void Suspend_WhenActive_TransitionsToSuspendedAndRecordsReason()
    {
        var permit = IssuedPermit();

        permit.Suspend("Effluent violation under investigation.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        permit.Status.Should().Be(DischargePermitStatus.Suspended);
        permit.LifecycleActionReason.Should().Be("Effluent violation under investigation.");
    }

    [Fact]
    public void Revoke_WhenSuspended_TransitionsToRevoked()
    {
        var permit = IssuedPermit();
        permit.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        permit.Revoke("Repeat violations.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        permit.Status.Should().Be(DischargePermitStatus.Revoked);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ThrowsInvalidStatusTransition()
    {
        var permit = IssuedPermit();
        permit.Revoke("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => permit.Revoke("reason again", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePermitInvalidStatusTransitionException>();
    }

    [Fact]
    public void Suspend_WhenRevoked_ThrowsInvalidStatusTransition()
    {
        var permit = IssuedPermit();
        permit.Revoke("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => permit.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePermitInvalidStatusTransitionException>();
    }

    [Fact]
    public void Renew_WhenRevoked_ThrowsInvalidStatusTransition()
    {
        var permit = IssuedPermit();
        permit.Revoke("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => permit.Renew(UtcTimestamp.From(FixedUtcNow.AddYears(1)), TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargePermitInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllRaisedEvents()
    {
        var permit = IssuedPermit();
        permit.DomainEvents.Should().NotBeEmpty();

        permit.ClearDomainEvents();

        permit.DomainEvents.Should().BeEmpty();
    }
}
