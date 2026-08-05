// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Backward Compatibility
//  File:           BackwardCompatibilityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Backward Compatibility test type: verifies additive-only command envelope
//                  evolution -- ApiVersionToken defaults to "v1" for callers that don't set it,
//                  RequestedAtUtc defaults to DateTime.UtcNow when omitted, and optional fields
//                  (SiteId/RouteId/PredictedPeakOccupancy) default safely to null so older callers
//                  that never populated them keep working. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class BackwardCompatibilityTests : UnitTestBase
{
    [Fact]
    public void DefineEvacuationRouteCommand_DefaultsApiVersionTokenToV1()
    {
        var command = new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value);

        command.ApiVersionToken.Should().Be("v1");
    }

    [Fact]
    public void DefineEvacuationRouteCommand_OptionalSiteId_DefaultsSafelyToNull()
    {
        // A caller built against an older client contract that never populated SiteId must still
        // compile and construct a valid command.
        var command = new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value);

        command.SiteId.Should().BeNull();
    }

    [Fact]
    public void RunEgressSimulationCommand_OptionalRouteIdAndPredictedPeakOccupancy_DefaultSafelyToNull()
    {
        var command = new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), null, "v2026.1.0", "Manual", 200, null, TestUserId.Value);

        command.RouteId.Should().BeNull();
        command.PredictedPeakOccupancy.Should().BeNull();
    }

    [Fact]
    public void DefineEvacuationZoneCommand_DefaultsApiVersionTokenToV1_AndRequestedAtUtcToNow()
    {
        var before = DateTime.UtcNow;
        var command = new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Zone", "v2026.1.0", 100, 80, TestUserId.Value);
        var after = DateTime.UtcNow;

        command.ApiVersionToken.Should().Be("v1");
        command.RequestedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void RecordRouteObstructionCommand_ActorIdMirrorsPerformedByUserId_ForOlderCallersThatOnlyPopulateOne()
    {
        var command = new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "Obstruction", "Minor", TestUserId.Value);

        command.ActorId.Should().Be(command.PerformedByUserId);
    }

    [Fact]
    public void LinkSimulationToDrillCommand_AllPositionalFieldsSurviveConstruction_NoSilentFieldDrop()
    {
        var commandId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var drillId = Guid.NewGuid();
        var command = new LinkSimulationToDrillCommand(
            commandId, TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, runId, drillId, TestUserId.Value);

        command.CommandId.Should().Be(commandId);
        command.EgressSimulationRunId.Should().Be(runId);
        command.DrillId.Should().Be(drillId);
    }
}
