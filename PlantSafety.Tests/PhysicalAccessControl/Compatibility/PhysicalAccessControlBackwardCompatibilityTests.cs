// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compatibility.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           PhysicalAccessControlBackwardCompatibilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Backward-compatibility / API-contract-shape tests: every PhysicalAccessControl
//                  command defaults ApiVersionToken to "v1" (the module's current API surface)
//                  and stamps a UTC RequestedAtUtc at construction, so callers built against the
//                  "v1" contract keep working without having to supply these envelope fields
//                  explicitly. Also proves aggregate ids are string-interoperable (ToString/Parse
//                  round-trip) -- required for the identifiers to remain compatible with any
//                  external system (badge vendors, IoT ingestion) that persists them as strings.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids;

namespace PlantSafety.Tests.PhysicalAccessControl.Compatibility;

public sealed class PhysicalAccessControlBackwardCompatibilityTests : UnitTestBase
{
    // ---- Every command defaults to the "v1" API contract --------------------------------

    [Fact]
    public void RegisterAccessPointCommand_ShouldDefaultApiVersionTokenToV1() =>
        new RegisterAccessPointCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "AP-1", "d", "BadgeReader", null, false, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void IssueCredentialCommand_ShouldDefaultApiVersionTokenToV1() =>
        new IssueCredentialCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "BADGE-1", Guid.NewGuid(), "Employee", "General", null, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void IngestAccessEventCommand_ShouldDefaultApiVersionTokenToV1() =>
        new IngestAccessEventCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, Guid.NewGuid(), null, null, "Granted", "In", FixedUtcNow, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void RegisterSafetyZoneCommand_ShouldDefaultApiVersionTokenToV1() =>
        new RegisterSafetyZoneCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "Z-1", "Zone", "Low", 1, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void DetectAccessAnomalyCommand_ShouldDefaultApiVersionTokenToV1() =>
        new DetectAccessAnomalyCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, null, Guid.NewGuid(), null, null, "Tailgating", 10m, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void DefineRestrictedZonePolicyCommand_ShouldDefaultApiVersionTokenToV1() =>
        new DefineRestrictedZonePolicyCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, Guid.NewGuid(), "Policy", "General", false, TestUserId.Value)
            .ApiVersionToken.Should().Be("v1");

    [Fact]
    public void RegisterAccessPointCommand_ShouldStampRequestedAtUtcAsUtcKind() =>
        new RegisterAccessPointCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "AP-1", "d", "BadgeReader", null, false, TestUserId.Value)
            .RequestedAtUtc.Kind.Should().Be(DateTimeKind.Utc);

    [Fact]
    public void RegisterAccessPointCommand_CausationId_ShouldRoundTripThroughExplicitInterfaceImplementation()
    {
        var causationId = Guid.NewGuid();
        var command = new RegisterAccessPointCommand(Guid.NewGuid(), TestCorrelationId.Value, causationId, TestTenantId.Value,
            null, "AP-1", "d", "BadgeReader", null, false, TestUserId.Value);

        ((PlantSafety.Application.Abstractions.Messaging.ICommand<PlantSafety.Application.Common.Results.Result<AccessPointActionResult>>)command)
            .CausationId.Should().Be(causationId);
    }

    // ---- Strong-typed identifiers remain string-interoperable ---------------------------

    [Fact]
    public void AccessPointId_ShouldRoundTripThroughStringParse()
    {
        var id = AccessPointId.New();
        AccessPointId.Parse(id.ToString()).Should().Be(id);
    }

    [Fact]
    public void CredentialId_ShouldRoundTripThroughStringParse()
    {
        var id = CredentialId.New();
        CredentialId.Parse(id.ToString()).Should().Be(id);
    }

    [Fact]
    public void AccessEventId_ShouldRoundTripThroughStringParse()
    {
        var id = AccessEventId.New();
        AccessEventId.Parse(id.ToString()).Should().Be(id);
    }

    [Fact]
    public void SafetyZoneId_ShouldRoundTripThroughStringParse()
    {
        var id = SafetyZoneId.New();
        SafetyZoneId.Parse(id.ToString()).Should().Be(id);
    }

    [Fact]
    public void AccessAnomalyId_ShouldRoundTripThroughStringParse()
    {
        var id = AccessAnomalyId.New();
        AccessAnomalyId.Parse(id.ToString()).Should().Be(id);
    }

    [Fact]
    public void RestrictedZonePolicyId_ShouldRoundTripThroughStringParse()
    {
        var id = RestrictedZonePolicyId.New();
        RestrictedZonePolicyId.Parse(id.ToString()).Should().Be(id);
    }

    // ---- Enum contract: existing numeric ordinals must never shift (append-only enums) --

    [Fact]
    public void AccessPointStatus_OrdinalsShouldRemainStable()
    {
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Registered).Should().Be(0);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Active).Should().Be(1);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Maintenance).Should().Be(2);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Decommissioned).Should().Be(3);
    }

    [Fact]
    public void CredentialStatus_OrdinalsShouldRemainStable()
    {
        ((int)PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums.CredentialStatus.Issued).Should().Be(0);
        ((int)PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums.CredentialStatus.Suspended).Should().Be(1);
        ((int)PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums.CredentialStatus.Revoked).Should().Be(2);
        ((int)PlantSafety.Domain.PhysicalAccessControl.Credentials.Enums.CredentialStatus.Expired).Should().Be(3);
    }

    [Fact]
    public void AccessAnomalyStatus_OrdinalsShouldRemainStable()
    {
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Detected).Should().Be(0);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Acknowledged).Should().Be(1);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Escalated).Should().Be(2);
        ((int)PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Resolved).Should().Be(3);
    }
}
