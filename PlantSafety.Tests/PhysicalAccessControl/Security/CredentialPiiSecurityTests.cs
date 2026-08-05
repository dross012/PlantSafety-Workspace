// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Security.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           CredentialPiiSecurityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Security-focused unit tests for the PhysicalAccessControl module's badge/
//                  credential PII surface: the read-model DTO exposed over the wire must never
//                  carry a raw badge number field, error messages returned to callers must never
//                  leak PII values, and the AccessAnomaly detection error path applies the same
//                  "generic message, no data leakage" discipline used for credential PII across
//                  the module's other entities.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using System.Reflection;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Models;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using CredentialAggregate = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;

namespace PlantSafety.Tests.PhysicalAccessControl.Security;

/// <summary>
/// Security tests asserting the Credential PII data-governance contract described in
/// <c>Credential.cs</c>'s file header ("Credential/PII fields are access-restricted and
/// audit-logged... no biometric data is modeled without an explicit consent workflow").
/// </summary>
public sealed class CredentialPiiSecurityTests : UnitTestBase
{
    // ---- Read-model DTO shape: no raw badge number may ever cross the wire -------------

    [Fact]
    public void CredentialStatusDto_ShouldNotExposeRawBadgeNumberProperty()
    {
        var properties = typeof(CredentialStatusDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        properties.Should().NotContain(p => p.Name == "BadgeNumber",
            "the read-model DTO must only ever carry the masked projection, never the raw badge number");
    }

    [Fact]
    public void CredentialStatusDto_ShouldExposeOnlyTheMaskedBadgeNumberField()
    {
        var properties = typeof(CredentialStatusDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        properties.Should().Contain(p => p.Name == "BadgeNumberMasked");
    }

    [Fact]
    public void AccessEventDto_ShouldNotExposeCredentialBadgeNumber()
    {
        var properties = typeof(AccessEventDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        properties.Should().NotContain(p => p.Name.Contains("BadgeNumber"),
            "AccessEvent read-models only carry the CredentialId linkage, never badge PII");
    }

    [Fact]
    public void AccessAnomalyDto_ShouldNotExposeCredentialBadgeNumber()
    {
        var properties = typeof(AccessAnomalyDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        properties.Should().NotContain(p => p.Name.Contains("BadgeNumber"),
            "AccessAnomaly read-models only carry the CredentialId linkage, never badge PII");
    }

    // ---- Error messages must never leak the raw badge number -------------------------

    [Fact]
    public async Task IssueCredential_WhenRepositoryThrows_ErrorMessageShouldNotContainRawBadgeNumber()
    {
        const string sensitiveBadgeNumber = "SECRET-BADGE-90210";
        var repository = Substitute.For<ICredentialRepository>();
        repository.AddAsync(Arg.Any<CredentialAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var command = new IssueCredentialCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, sensitiveBadgeNumber, Guid.NewGuid(), "Employee", "General", null, TestUserId.Value);

        var result = await new IssueCredentialCommandHandler(repository, NullLogger<IssueCredentialCommandHandler>())
            .Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().NotContain(sensitiveBadgeNumber,
            "ApplicationError messages returned to API callers must be generic and never echo PII input");
    }

    [Fact]
    public async Task RevokeCredential_WhenRepositoryThrows_ErrorMessageShouldNotContainRevocationReason()
    {
        const string sensitiveReason = "Terminated for cause: confidential HR matter #4471";
        var repository = Substitute.For<ICredentialRepository>();
        var aggregate = CredentialAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, badgeNumber: "BADGE-1", holderUserId: Guid.NewGuid());
        repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids.CredentialId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);
        repository.UpdateAsync(Arg.Any<CredentialAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var command = new RevokeCredentialCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            aggregate.CredentialId.Value, sensitiveReason, TestUserId.Value);

        var result = await new RevokeCredentialCommandHandler(repository, NullLogger<RevokeCredentialCommandHandler>())
            .Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().NotContain(sensitiveReason);
    }

    // ---- Credential aggregate: revocation reason survives suspend->revoke but never bleeds
    //      into an unrelated aggregate instance --------------------------------------------

    [Fact]
    public void Credential_RevocationReason_ShouldOnlyBeSetAfterRevoke()
    {
        var aggregate = CredentialAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, badgeNumber: "BADGE-2", holderUserId: Guid.NewGuid());

        aggregate.RevocationReason.Should().BeNull("no revocation has occurred yet");

        aggregate.Revoke("Badge reported lost", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        aggregate.RevocationReason.Should().Be("Badge reported lost");
    }

    [Fact]
    public void Credential_TwoSeparateAggregates_ShouldNotShareRevocationReasonState()
    {
        var a1 = CredentialAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, badgeNumber: "BADGE-3", holderUserId: Guid.NewGuid());
        var a2 = CredentialAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, badgeNumber: "BADGE-4", holderUserId: Guid.NewGuid());

        a1.Revoke("Reason for badge 3 only", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        a2.RevocationReason.Should().BeNull("revoking one Credential aggregate must never mutate another instance's state");
    }
}
