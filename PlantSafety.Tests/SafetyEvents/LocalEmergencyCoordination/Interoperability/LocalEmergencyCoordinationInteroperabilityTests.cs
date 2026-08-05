// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Interoperability
//  Layer:          Tests (Interoperability)
//  File:           LocalEmergencyCoordinationInteroperabilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Interoperability tests verifying the module's cross-module correlation surface:
//                  the nullable IncidentId used to correlate with SafetyEvents.EmergencyManagement /
//                  CrisisManagement incidents, and that read-model DTOs round-trip cleanly through
//                  System.Text.Json (the wire format used by the API/UI and by any downstream
//                  consumer of the module's outbox events). Isolated, deterministic, no I/O,
//                  no network.
// ============================================================================
using System.Text.Json;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Interoperability;

public sealed class LocalEmergencyCoordinationInteroperabilityTests : UnitTestBase
{
    [Fact]
    public void Aggregate_WithoutIncidentId_InteroperatesAsEnterpriseLevelCase()
    {
        // A case opened with no IncidentId is a valid standalone coordination case, not correlated
        // to any specific EmergencyManagement/CrisisManagement incident.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(incidentId: null);

        agg.IncidentId.Should().BeNull();
    }

    [Fact]
    public void Aggregate_WithIncidentId_CorrelatesToExternalSafetyEventsIncident()
    {
        var incidentId = Guid.NewGuid();
        var agg = LocalEmergencyCoordinationTestSupport.NewCase(incidentId: incidentId);

        agg.IncidentId.Should().Be(incidentId, "IncidentId is the cross-module correlation key back to EmergencyManagement/CrisisManagement");
    }

    [Fact]
    public void LocalEmergencyCoordinationItemDto_RoundTripsThroughSystemTextJson()
    {
        var dto = new LocalEmergencyCoordinationItemDto
        {
            LocalEmergencyCoordinationId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            SiteId = Guid.NewGuid(),
            IncidentId = Guid.NewGuid(),
            CaseName = "Wildfire Mutual Aid Case",
            Status = "Active",
            CreatedAtUtc = FixedUtcNow,
            CreatedBy = TestUserId.Value,
            ETag = "0x0000000000001234",
        };

        var json = JsonSerializer.Serialize(dto);
        var roundTripped = JsonSerializer.Deserialize<LocalEmergencyCoordinationItemDto>(json);

        roundTripped.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void MutualAidAgreementItemDto_RoundTripsThroughSystemTextJson()
    {
        var dto = new MutualAidAgreementItemDto
        {
            MutualAidAgreementId = Guid.NewGuid(),
            LocalEmergencyCoordinationId = Guid.NewGuid(),
            TenantId = TestTenantId.Value,
            AgencyName = "Metro Fire Dept",
            Discipline = "Fire",
            DocumentVersion = 2,
            EffectiveFromUtc = FixedUtcNow,
            RenewalDueAtUtc = FixedUtcNow.AddYears(1),
            RenewalStatus = "Current",
            EstablishedAtUtc = FixedUtcNow,
            EstablishedBy = TestUserId.Value,
        };

        var json = JsonSerializer.Serialize(dto);
        var roundTripped = JsonSerializer.Deserialize<MutualAidAgreementItemDto>(json);

        roundTripped.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void JointResponsePlan_IncidentId_InteroperatesWithNullableGuidAcrossHandlerBoundary()
    {
        // ActivateJointResponsePlanCommand.IncidentId is Guid? end-to-end (command -> aggregate ->
        // child entity -> event), so a null incident correlation must survive the round trip
        // unchanged rather than collapsing to Guid.Empty.
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var plan = agg.ActivateJointResponsePlan(null, "Standalone Plan", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        plan.IncidentId.Should().BeNull();
    }
}
