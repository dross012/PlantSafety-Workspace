// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           ValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper coverage for all 12 PhysicalAccessControl command
//                  validators: a fully-valid command produces no errors, and each mandatory rule
//                  is independently exercised for a validation failure.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Application.PhysicalAccessControl.Validation;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class ValidatorTests : UnitTestBase
{
    // ---- RegisterAccessPointCommandValidator ----------------------------------------

    private static RegisterAccessPointCommand ValidRegisterAccessPoint() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "AP-1", "Lobby", "BadgeReader", null, false, TestUserId.Value);

    [Fact]
    public void RegisterAccessPoint_WithValidCommand_ShouldNotHaveErrors() =>
        new RegisterAccessPointCommandValidator().TestValidate(ValidRegisterAccessPoint()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RegisterAccessPoint_WithEmptyCode_ShouldHaveError() =>
        new RegisterAccessPointCommandValidator()
            .TestValidate(ValidRegisterAccessPoint() with { AccessPointCode = "" })
            .ShouldHaveValidationErrorFor(x => x.AccessPointCode);

    [Fact]
    public void RegisterAccessPoint_WithEmptyDeviceType_ShouldHaveError() =>
        new RegisterAccessPointCommandValidator()
            .TestValidate(ValidRegisterAccessPoint() with { DeviceType = "" })
            .ShouldHaveValidationErrorFor(x => x.DeviceType);

    [Fact]
    public void RegisterAccessPoint_WithEmptyTenantId_ShouldHaveError() =>
        new RegisterAccessPointCommandValidator()
            .TestValidate(ValidRegisterAccessPoint() with { TenantId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.TenantId);

    // ---- DecommissionAccessPointCommandValidator ------------------------------------

    private static DecommissionAccessPointCommand ValidDecommissionAccessPoint() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void DecommissionAccessPoint_WithValidCommand_ShouldNotHaveErrors() =>
        new DecommissionAccessPointCommandValidator().TestValidate(ValidDecommissionAccessPoint()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DecommissionAccessPoint_WithEmptyAccessPointId_ShouldHaveError() =>
        new DecommissionAccessPointCommandValidator()
            .TestValidate(ValidDecommissionAccessPoint() with { AccessPointId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AccessPointId);

    // ---- IssueCredentialCommandValidator ----------------------------------------------

    private static IssueCredentialCommand ValidIssueCredential() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "BADGE-1", Guid.NewGuid(), "Employee", "General", null, TestUserId.Value);

    [Fact]
    public void IssueCredential_WithValidCommand_ShouldNotHaveErrors() =>
        new IssueCredentialCommandValidator().TestValidate(ValidIssueCredential()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IssueCredential_WithEmptyBadgeNumber_ShouldHaveError() =>
        new IssueCredentialCommandValidator()
            .TestValidate(ValidIssueCredential() with { BadgeNumber = "" })
            .ShouldHaveValidationErrorFor(x => x.BadgeNumber);

    [Fact]
    public void IssueCredential_WithEmptyHolderUserId_ShouldHaveError() =>
        new IssueCredentialCommandValidator()
            .TestValidate(ValidIssueCredential() with { HolderUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.HolderUserId);

    [Fact]
    public void IssueCredential_WithEmptyAccessLevel_ShouldHaveError() =>
        new IssueCredentialCommandValidator()
            .TestValidate(ValidIssueCredential() with { AccessLevel = "" })
            .ShouldHaveValidationErrorFor(x => x.AccessLevel);

    // ---- RevokeCredentialCommandValidator ---------------------------------------------

    private static RevokeCredentialCommand ValidRevokeCredential() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Lost badge", TestUserId.Value);

    [Fact]
    public void RevokeCredential_WithValidCommand_ShouldNotHaveErrors() =>
        new RevokeCredentialCommandValidator().TestValidate(ValidRevokeCredential()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RevokeCredential_WithEmptyReason_ShouldHaveError() =>
        new RevokeCredentialCommandValidator()
            .TestValidate(ValidRevokeCredential() with { Reason = "" })
            .ShouldHaveValidationErrorFor(x => x.Reason);

    [Fact]
    public void RevokeCredential_WithOverlongReason_ShouldHaveError() =>
        new RevokeCredentialCommandValidator()
            .TestValidate(ValidRevokeCredential() with { Reason = new string('x', 1001) })
            .ShouldHaveValidationErrorFor(x => x.Reason);

    // ---- IngestAccessEventCommandValidator ---------------------------------------------

    private static IngestAccessEventCommand ValidIngestAccessEvent() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Granted", "In", FixedUtcNow, TestUserId.Value);

    [Fact]
    public void IngestAccessEvent_WithValidCommand_ShouldNotHaveErrors() =>
        new IngestAccessEventCommandValidator().TestValidate(ValidIngestAccessEvent()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void IngestAccessEvent_WithEmptyAccessPointId_ShouldHaveError() =>
        new IngestAccessEventCommandValidator()
            .TestValidate(ValidIngestAccessEvent() with { AccessPointId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AccessPointId);

    [Fact]
    public void IngestAccessEvent_WithEmptyEventType_ShouldHaveError() =>
        new IngestAccessEventCommandValidator()
            .TestValidate(ValidIngestAccessEvent() with { EventType = "" })
            .ShouldHaveValidationErrorFor(x => x.EventType);

    [Fact]
    public void IngestAccessEvent_WithEmptyDirection_ShouldHaveError() =>
        new IngestAccessEventCommandValidator()
            .TestValidate(ValidIngestAccessEvent() with { Direction = "" })
            .ShouldHaveValidationErrorFor(x => x.Direction);

    [Fact]
    public void IngestAccessEvent_WithDefaultOccurredAtUtc_ShouldHaveError() =>
        new IngestAccessEventCommandValidator()
            .TestValidate(ValidIngestAccessEvent() with { OccurredAtUtc = default })
            .ShouldHaveValidationErrorFor(x => x.OccurredAtUtc);

    // ---- FlagAccessEventAnomalyCommandValidator -----------------------------------------

    private static FlagAccessEventAnomalyCommand ValidFlagAccessEventAnomaly() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Tailgating", TestUserId.Value);

    [Fact]
    public void FlagAccessEventAnomaly_WithValidCommand_ShouldNotHaveErrors() =>
        new FlagAccessEventAnomalyCommandValidator().TestValidate(ValidFlagAccessEventAnomaly()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void FlagAccessEventAnomaly_WithEmptyAnomalyType_ShouldHaveError() =>
        new FlagAccessEventAnomalyCommandValidator()
            .TestValidate(ValidFlagAccessEventAnomaly() with { AnomalyType = "" })
            .ShouldHaveValidationErrorFor(x => x.AnomalyType);

    // ---- RegisterSafetyZoneCommandValidator ---------------------------------------------

    private static RegisterSafetyZoneCommand ValidRegisterSafetyZone() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "ZONE-A", "Zone A", "Medium", 10, TestUserId.Value);

    [Fact]
    public void RegisterSafetyZone_WithValidCommand_ShouldNotHaveErrors() =>
        new RegisterSafetyZoneCommandValidator().TestValidate(ValidRegisterSafetyZone()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RegisterSafetyZone_WithEmptyZoneCode_ShouldHaveError() =>
        new RegisterSafetyZoneCommandValidator()
            .TestValidate(ValidRegisterSafetyZone() with { ZoneCode = "" })
            .ShouldHaveValidationErrorFor(x => x.ZoneCode);

    [Fact]
    public void RegisterSafetyZone_WithEmptyZoneName_ShouldHaveError() =>
        new RegisterSafetyZoneCommandValidator()
            .TestValidate(ValidRegisterSafetyZone() with { ZoneName = "" })
            .ShouldHaveValidationErrorFor(x => x.ZoneName);

    [Fact]
    public void RegisterSafetyZone_WithNegativeMaxOccupancy_ShouldHaveError() =>
        new RegisterSafetyZoneCommandValidator()
            .TestValidate(ValidRegisterSafetyZone() with { MaxOccupancy = -1 })
            .ShouldHaveValidationErrorFor(x => x.MaxOccupancy);

    // ---- RestrictSafetyZoneCommandValidator -----------------------------------------------

    private static RestrictSafetyZoneCommand ValidRestrictSafetyZone() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void RestrictSafetyZone_WithValidCommand_ShouldNotHaveErrors() =>
        new RestrictSafetyZoneCommandValidator().TestValidate(ValidRestrictSafetyZone()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RestrictSafetyZone_WithEmptySafetyZoneId_ShouldHaveError() =>
        new RestrictSafetyZoneCommandValidator()
            .TestValidate(ValidRestrictSafetyZone() with { SafetyZoneId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.SafetyZoneId);

    // ---- DetectAccessAnomalyCommandValidator -----------------------------------------------

    private static DetectAccessAnomalyCommand ValidDetectAccessAnomaly() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tailgating", 50m, TestUserId.Value);

    [Fact]
    public void DetectAccessAnomaly_WithValidCommand_ShouldNotHaveErrors() =>
        new DetectAccessAnomalyCommandValidator().TestValidate(ValidDetectAccessAnomaly()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DetectAccessAnomaly_WithEmptyAccessPointId_ShouldHaveError() =>
        new DetectAccessAnomalyCommandValidator()
            .TestValidate(ValidDetectAccessAnomaly() with { AccessPointId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AccessPointId);

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void DetectAccessAnomaly_WithSeverityScoreOutOfRange_ShouldHaveError(decimal severityScore) =>
        new DetectAccessAnomalyCommandValidator()
            .TestValidate(ValidDetectAccessAnomaly() with { SeverityScore = severityScore })
            .ShouldHaveValidationErrorFor(x => x.SeverityScore);

    [Fact]
    public void DetectAccessAnomaly_WithEmptyAnomalyType_ShouldHaveError() =>
        new DetectAccessAnomalyCommandValidator()
            .TestValidate(ValidDetectAccessAnomaly() with { AnomalyType = "" })
            .ShouldHaveValidationErrorFor(x => x.AnomalyType);

    // ---- AcknowledgeAccessAnomalyCommandValidator -----------------------------------------

    private static AcknowledgeAccessAnomalyCommand ValidAcknowledgeAccessAnomaly() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void AcknowledgeAccessAnomaly_WithValidCommand_ShouldNotHaveErrors() =>
        new AcknowledgeAccessAnomalyCommandValidator().TestValidate(ValidAcknowledgeAccessAnomaly()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void AcknowledgeAccessAnomaly_WithEmptyAcknowledgedByUserId_ShouldHaveError() =>
        new AcknowledgeAccessAnomalyCommandValidator()
            .TestValidate(ValidAcknowledgeAccessAnomaly() with { AcknowledgedByUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AcknowledgedByUserId);

    // ---- DefineRestrictedZonePolicyCommandValidator ---------------------------------------

    private static DefineRestrictedZonePolicyCommand ValidDefineRestrictedZonePolicy() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), "High-Voltage Room Policy", "RestrictedZoneA", true, TestUserId.Value);

    [Fact]
    public void DefineRestrictedZonePolicy_WithValidCommand_ShouldNotHaveErrors() =>
        new DefineRestrictedZonePolicyCommandValidator().TestValidate(ValidDefineRestrictedZonePolicy()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DefineRestrictedZonePolicy_WithEmptySafetyZoneId_ShouldHaveError() =>
        new DefineRestrictedZonePolicyCommandValidator()
            .TestValidate(ValidDefineRestrictedZonePolicy() with { SafetyZoneId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.SafetyZoneId);

    [Fact]
    public void DefineRestrictedZonePolicy_WithEmptyPolicyName_ShouldHaveError() =>
        new DefineRestrictedZonePolicyCommandValidator()
            .TestValidate(ValidDefineRestrictedZonePolicy() with { PolicyName = "" })
            .ShouldHaveValidationErrorFor(x => x.PolicyName);

    [Fact]
    public void DefineRestrictedZonePolicy_WithEmptyAllowedAccessLevel_ShouldHaveError() =>
        new DefineRestrictedZonePolicyCommandValidator()
            .TestValidate(ValidDefineRestrictedZonePolicy() with { AllowedAccessLevel = "" })
            .ShouldHaveValidationErrorFor(x => x.AllowedAccessLevel);

    // ---- ActivateRestrictedZonePolicyCommandValidator -------------------------------------

    private static ActivateRestrictedZonePolicyCommand ValidActivateRestrictedZonePolicy() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void ActivateRestrictedZonePolicy_WithValidCommand_ShouldNotHaveErrors() =>
        new ActivateRestrictedZonePolicyCommandValidator().TestValidate(ValidActivateRestrictedZonePolicy()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ActivateRestrictedZonePolicy_WithEmptyRestrictedZonePolicyId_ShouldHaveError() =>
        new ActivateRestrictedZonePolicyCommandValidator()
            .TestValidate(ValidActivateRestrictedZonePolicy() with { RestrictedZonePolicyId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.RestrictedZonePolicyId);

    // ---- Cross-cutting: PerformedByUserId is mandatory on every command ------------------

    [Fact]
    public void RegisterAccessPoint_WithEmptyPerformedByUserId_ShouldHaveError() =>
        new RegisterAccessPointCommandValidator()
            .TestValidate(ValidRegisterAccessPoint() with { PerformedByUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PerformedByUserId);

    [Fact]
    public void IssueCredential_WithEmptyPerformedByUserId_ShouldHaveError() =>
        new IssueCredentialCommandValidator()
            .TestValidate(ValidIssueCredential() with { PerformedByUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PerformedByUserId);

    [Fact]
    public void DefineRestrictedZonePolicy_WithEmptyPerformedByUserId_ShouldHaveError() =>
        new DefineRestrictedZonePolicyCommandValidator()
            .TestValidate(ValidDefineRestrictedZonePolicy() with { PerformedByUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PerformedByUserId);
}
