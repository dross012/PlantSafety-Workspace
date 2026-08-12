// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Scenario.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           PhysicalAccessControlScenarioTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    End-to-end business scenarios exercised entirely against in-memory,
//                  NSubstitute-backed repositories (no DB), chaining multiple command handlers
//                  the way the real workflow would: register infrastructure -> issue a badge ->
//                  ingest an access event -> detect/flag an anomaly -> triage it to resolution.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using AccessAnomalyAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly;
using AccessEventAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent;
using AccessPointAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint;
using CredentialAggregate = PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential;
using RestrictedZonePolicyAggregate = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy;
using SafetyZoneAggregate = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone;

namespace PlantSafety.Tests.PhysicalAccessControl.Scenario;

/// <summary>
/// Multi-step business scenarios spanning all 6 PhysicalAccessControl entities, run entirely
/// against in-memory fakes to prove the handlers compose into the real-world workflows the
/// module was designed for.
/// </summary>
public sealed class PhysicalAccessControlScenarioTests : UnitTestBase
{
    [Fact(DisplayName = "SCEN-PAC-001: Badge issued, presented at a restricted zone after a policy is enforced, and a violation is detected, escalated, and resolved")]
    public async Task RestrictedZoneAccessViolationScenario_ShouldFlowThroughAllSixEntities()
    {
        // ---- Arrange: repositories shared across the scenario's step sequence ----------
        var accessPointRepo = Substitute.For<IAccessPointRepository>();
        var credentialRepo = Substitute.For<ICredentialRepository>();
        var accessEventRepo = Substitute.For<IAccessEventRepository>();
        var safetyZoneRepo = Substitute.For<ISafetyZoneRepository>();
        var accessAnomalyRepo = Substitute.For<IAccessAnomalyRepository>();
        var policyRepo = Substitute.For<IRestrictedZonePolicyRepository>();

        // Discarded, not awaited. These Arg.Do calls CONFIGURE the substitute -- they register a
        // callback that captures the aggregate when the handler later calls AddAsync -- rather
        // than performing an operation. The Task they return is NSubstitute's default and carries
        // no work, so awaiting it would suspend on nothing and imply a sequencing guarantee that
        // does not exist. The discard says the return value is deliberately ignored, which is
        // what CS4014 asks for here.
        SafetyZoneAggregate? zone = null;
        _ = safetyZoneRepo.AddAsync(Arg.Do<SafetyZoneAggregate>(a => zone = a), Arg.Any<CancellationToken>());

        AccessPointAggregate? accessPoint = null;
        _ = accessPointRepo.AddAsync(Arg.Do<AccessPointAggregate>(a => accessPoint = a), Arg.Any<CancellationToken>());

        CredentialAggregate? credential = null;
        _ = credentialRepo.AddAsync(Arg.Do<CredentialAggregate>(a => credential = a), Arg.Any<CancellationToken>());

        RestrictedZonePolicyAggregate? policy = null;
        _ = policyRepo.AddAsync(Arg.Do<RestrictedZonePolicyAggregate>(a => policy = a), Arg.Any<CancellationToken>());

        AccessEventAggregate? accessEvent = null;
        _ = accessEventRepo.AddAsync(Arg.Do<AccessEventAggregate>(a => accessEvent = a), Arg.Any<CancellationToken>());

        AccessAnomalyAggregate? anomaly = null;
        _ = accessAnomalyRepo.AddAsync(Arg.Do<AccessAnomalyAggregate>(a => anomaly = a), Arg.Any<CancellationToken>());

        // ---- Step 1: register the SafetyZone --------------------------------------------
        var zoneResult = await new RegisterSafetyZoneCommandHandler(safetyZoneRepo, NullLogger<RegisterSafetyZoneCommandHandler>())
            .Handle(new RegisterSafetyZoneCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "ZONE-HV", "High-Voltage Substation", "Critical", 2, TestUserId.Value),
                CancellationToken.None);
        zoneResult.IsSuccess.Should().BeTrue();
        zone.Should().NotBeNull();

        // ---- Step 2: register the AccessPoint guarding that zone ------------------------
        var accessPointResult = await new RegisterAccessPointCommandHandler(accessPointRepo, NullLogger<RegisterAccessPointCommandHandler>())
            .Handle(new RegisterAccessPointCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "AP-HV-1", "Substation mantrap", "MantrapDoor", zone!.SafetyZoneId.Value, false, TestUserId.Value),
                CancellationToken.None);
        accessPointResult.IsSuccess.Should().BeTrue();

        // ---- Step 3: define and activate the RestrictedZonePolicy -----------------------
        var defineResult = await new DefineRestrictedZonePolicyCommandHandler(policyRepo, NullLogger<DefineRestrictedZonePolicyCommandHandler>())
            .Handle(new DefineRestrictedZonePolicyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, zone.SafetyZoneId.Value, "High-Voltage Restricted Policy", "HighVoltageClearance", true, TestUserId.Value),
                CancellationToken.None);
        defineResult.IsSuccess.Should().BeTrue();
        policy.Should().NotBeNull();

        policyRepo.GetByIdAsync(policy!.RestrictedZonePolicyId, Arg.Any<CancellationToken>()).Returns(policy);
        var activateResult = await new ActivateRestrictedZonePolicyCommandHandler(policyRepo, NullLogger<ActivateRestrictedZonePolicyCommandHandler>())
            .Handle(new ActivateRestrictedZonePolicyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                policy.RestrictedZonePolicyId.Value, TestUserId.Value),
                CancellationToken.None);
        activateResult.IsSuccess.Should().BeTrue();
        activateResult.Value.Status.Should().Be("Active");

        // ---- Step 4: issue a General-level credential (insufficient for this zone) ------
        var issueResult = await new IssueCredentialCommandHandler(credentialRepo, NullLogger<IssueCredentialCommandHandler>())
            .Handle(new IssueCredentialCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "BADGE-VISITOR-1", Guid.NewGuid(), "Visitor", "General", null, TestUserId.Value),
                CancellationToken.None);
        issueResult.IsSuccess.Should().BeTrue();
        credential.Should().NotBeNull();

        // ---- Step 5: ingest the access event (the visitor badges in anyway) -------------
        var ingestResult = await new IngestAccessEventCommandHandler(accessEventRepo, NullLogger<IngestAccessEventCommandHandler>())
            .Handle(new IngestAccessEventCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, accessPoint!.AccessPointId.Value, credential!.CredentialId.Value, zone.SafetyZoneId.Value,
                "Granted", "In", FixedUtcNow, TestUserId.Value),
                CancellationToken.None);
        ingestResult.IsSuccess.Should().BeTrue();
        accessEvent.Should().NotBeNull();

        // ---- Step 6: flag the event and detect the corresponding anomaly ----------------
        accessEventRepo.GetByIdAsync(accessEvent!.AccessEventId, Arg.Any<CancellationToken>()).Returns(accessEvent);
        var flagResult = await new FlagAccessEventAnomalyCommandHandler(accessEventRepo, NullLogger<FlagAccessEventAnomalyCommandHandler>())
            .Handle(new FlagAccessEventAnomalyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                accessEvent.AccessEventId.Value, "RestrictedZoneViolation", TestUserId.Value),
                CancellationToken.None);
        flagResult.IsSuccess.Should().BeTrue();
        flagResult.Value.Status.Should().Be("Flagged");

        var detectResult = await new DetectAccessAnomalyCommandHandler(accessAnomalyRepo, NullLogger<DetectAccessAnomalyCommandHandler>())
            .Handle(new DetectAccessAnomalyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, accessEvent.AccessEventId.Value, accessPoint.AccessPointId.Value, credential.CredentialId.Value,
                zone.SafetyZoneId.Value, "RestrictedZoneViolation", 85m, TestUserId.Value),
                CancellationToken.None);
        detectResult.IsSuccess.Should().BeTrue();
        anomaly.Should().NotBeNull();
        anomaly!.SeverityScore.Should().Be(85m);

        // ---- Step 7: acknowledge, escalate, and resolve the anomaly ---------------------
        accessAnomalyRepo.GetByIdAsync(anomaly.AccessAnomalyId, Arg.Any<CancellationToken>()).Returns(anomaly);
        var ackByUserId = Guid.NewGuid();
        var acknowledgeResult = await new AcknowledgeAccessAnomalyCommandHandler(accessAnomalyRepo, NullLogger<AcknowledgeAccessAnomalyCommandHandler>())
            .Handle(new AcknowledgeAccessAnomalyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                anomaly.AccessAnomalyId.Value, ackByUserId, TestUserId.Value),
                CancellationToken.None);
        acknowledgeResult.IsSuccess.Should().BeTrue();
        anomaly.AcknowledgedByUserId.Should().Be(ackByUserId);

        anomaly.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        anomaly.Status.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Escalated);

        anomaly.Resolve("Security escalation confirmed unauthorized visitor access; badge revoked, incident logged.",
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        anomaly.Status.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyStatus.Resolved);

        // ---- Step 8: revoke the offending credential as the closing remediation ---------
        credentialRepo.GetByIdAsync(credential.CredentialId, Arg.Any<CancellationToken>()).Returns(credential);
        var revokeResult = await new RevokeCredentialCommandHandler(credentialRepo, NullLogger<RevokeCredentialCommandHandler>())
            .Handle(new RevokeCredentialCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                credential.CredentialId.Value, "Restricted-zone violation incident PAC-SCEN-001", TestUserId.Value),
                CancellationToken.None);

        revokeResult.IsSuccess.Should().BeTrue();
        revokeResult.Value.Status.Should().Be("Revoked");
    }

    [Fact(DisplayName = "SCEN-PAC-002: An AccessPoint's full device lifecycle interleaves with occupancy tracking on the zone it guards")]
    public async Task AccessPointMaintenanceAndOccupancyScenario_ShouldReflectAcrossBothAggregates()
    {
        var accessPointRepo = Substitute.For<IAccessPointRepository>();
        var safetyZoneRepo = Substitute.For<ISafetyZoneRepository>();

        SafetyZoneAggregate? zone = null;
        _ = safetyZoneRepo.AddAsync(Arg.Do<SafetyZoneAggregate>(a => zone = a), Arg.Any<CancellationToken>());

        var zoneResult = await new RegisterSafetyZoneCommandHandler(safetyZoneRepo, NullLogger<RegisterSafetyZoneCommandHandler>())
            .Handle(new RegisterSafetyZoneCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "ZONE-PROD", "Production Floor", "Medium", 50, TestUserId.Value),
                CancellationToken.None);
        zoneResult.IsSuccess.Should().BeTrue();

        AccessPointAggregate? accessPoint = null;
        _ = accessPointRepo.AddAsync(Arg.Do<AccessPointAggregate>(a => accessPoint = a), Arg.Any<CancellationToken>());

        await new RegisterAccessPointCommandHandler(accessPointRepo, NullLogger<RegisterAccessPointCommandHandler>())
            .Handle(new RegisterAccessPointCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, "AP-PROD-1", "Production floor turnstile", "Turnstile", zone!.SafetyZoneId.Value, false, TestUserId.Value),
                CancellationToken.None);

        // Occupancy climbs as workers badge in through the access point.
        zone.UpdateOccupancy(18, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        zone.CurrentOccupancy.Should().Be(18);

        // The access point goes down for maintenance mid-shift.
        accessPointRepo.GetByIdAsync(accessPoint!.AccessPointId, Arg.Any<CancellationToken>()).Returns(accessPoint);
        accessPoint.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var maintenanceResult = accessPoint.Status;
        accessPoint.ScheduleMaintenance(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        accessPoint.Status.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Maintenance);
        maintenanceResult.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums.AccessPointStatus.Active);

        // Occupancy does not silently reset just because a single access point is down --
        // zone occupancy is a separate concern from any one device's lifecycle state.
        zone.CurrentOccupancy.Should().Be(18);

        // The device is brought back online, and the shift ends, so occupancy drops to zero.
        accessPoint.Activate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        zone.UpdateOccupancy(0, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        zone.CurrentOccupancy.Should().Be(0);
    }
}
