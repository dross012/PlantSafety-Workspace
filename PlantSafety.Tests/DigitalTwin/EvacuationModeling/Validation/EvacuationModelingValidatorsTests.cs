// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Validation
//  Layer:          Tests (Unit)
//  File:           EvacuationModelingValidatorsTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for every FluentValidation validator in
//                  DigitalTwin.EvacuationModeling.Validation (8 command validators across the
//                  module's 4 aggregates). Each validator gets a "valid command passes" case and
//                  one failing case per required rule. Isolated, deterministic, <50ms per test.
//                  No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Validation;

public sealed class EvacuationModelingValidatorsTests : UnitTestBase
{
    // ---- DefineEvacuationRouteCommandValidator ------------------------------------

    private static DefineEvacuationRouteCommand ValidDefineRoute() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Corridor A", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 120, TestUserId.Value);

    [Fact]
    public void DefineEvacuationRouteCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new DefineEvacuationRouteCommandValidator().TestValidate(ValidDefineRoute()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DefineEvacuationRouteCommandValidator_WithBlankRouteName_ShouldHaveError() =>
        new DefineEvacuationRouteCommandValidator().TestValidate(ValidDefineRoute() with { RouteName = "" })
            .ShouldHaveValidationErrorFor(c => c.RouteName);

    [Fact]
    public void DefineEvacuationRouteCommandValidator_WithEmptyZoneId_ShouldHaveError() =>
        new DefineEvacuationRouteCommandValidator().TestValidate(ValidDefineRoute() with { ZoneId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.ZoneId);

    [Fact]
    public void DefineEvacuationRouteCommandValidator_WithBlankTwinModelVersion_ShouldHaveError() =>
        new DefineEvacuationRouteCommandValidator().TestValidate(ValidDefineRoute() with { TwinModelVersion = "" })
            .ShouldHaveValidationErrorFor(c => c.TwinModelVersion);

    [Fact]
    public void DefineEvacuationRouteCommandValidator_WithNegativeEgressTime_ShouldHaveError() =>
        new DefineEvacuationRouteCommandValidator().TestValidate(ValidDefineRoute() with { EstimatedEgressTimeSeconds = -1 })
            .ShouldHaveValidationErrorFor(c => c.EstimatedEgressTimeSeconds);

    // ---- RetireEvacuationRouteCommandValidator ------------------------------------

    private static RetireEvacuationRouteCommand ValidRetireRoute() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void RetireEvacuationRouteCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new RetireEvacuationRouteCommandValidator().TestValidate(ValidRetireRoute()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RetireEvacuationRouteCommandValidator_WithEmptyRouteId_ShouldHaveError() =>
        new RetireEvacuationRouteCommandValidator().TestValidate(ValidRetireRoute() with { EvacuationRouteId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.EvacuationRouteId);

    // ---- RecordRouteObstructionCommandValidator -----------------------------------

    private static RecordRouteObstructionCommand ValidRecordObstruction() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Debris blocking corridor", "Moderate", TestUserId.Value);

    [Fact]
    public void RecordRouteObstructionCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new RecordRouteObstructionCommandValidator().TestValidate(ValidRecordObstruction()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RecordRouteObstructionCommandValidator_WithBlankDescription_ShouldHaveError() =>
        new RecordRouteObstructionCommandValidator().TestValidate(ValidRecordObstruction() with { Description = "" })
            .ShouldHaveValidationErrorFor(c => c.Description);

    [Fact]
    public void RecordRouteObstructionCommandValidator_WithBlankSeverity_ShouldHaveError() =>
        new RecordRouteObstructionCommandValidator().TestValidate(ValidRecordObstruction() with { Severity = "" })
            .ShouldHaveValidationErrorFor(c => c.Severity);

    [Fact]
    public void RecordRouteObstructionCommandValidator_WithDescriptionOver1000Chars_ShouldHaveError() =>
        new RecordRouteObstructionCommandValidator().TestValidate(ValidRecordObstruction() with { Description = new string('x', 1001) })
            .ShouldHaveValidationErrorFor(c => c.Description);

    // ---- ClearRouteObstructionCommandValidator ------------------------------------

    private static ClearRouteObstructionCommand ValidClearObstruction() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void ClearRouteObstructionCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new ClearRouteObstructionCommandValidator().TestValidate(ValidClearObstruction()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ClearRouteObstructionCommandValidator_WithEmptyObstructionId_ShouldHaveError() =>
        new ClearRouteObstructionCommandValidator().TestValidate(ValidClearObstruction() with { RouteObstructionId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.RouteObstructionId);

    // ---- DefineEvacuationZoneCommandValidator -------------------------------------

    private static DefineEvacuationZoneCommand ValidDefineZone() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Zone A", "v2026.1.0", 200, 150, TestUserId.Value);

    [Fact]
    public void DefineEvacuationZoneCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new DefineEvacuationZoneCommandValidator().TestValidate(ValidDefineZone()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void DefineEvacuationZoneCommandValidator_WithBlankZoneName_ShouldHaveError() =>
        new DefineEvacuationZoneCommandValidator().TestValidate(ValidDefineZone() with { ZoneName = "" })
            .ShouldHaveValidationErrorFor(c => c.ZoneName);

    [Fact]
    public void DefineEvacuationZoneCommandValidator_WithNegativeZoneCapacity_ShouldHaveError() =>
        new DefineEvacuationZoneCommandValidator().TestValidate(ValidDefineZone() with { ZoneCapacity = -1 })
            .ShouldHaveValidationErrorFor(c => c.ZoneCapacity);

    [Fact]
    public void DefineEvacuationZoneCommandValidator_WithNegativeOccupancyThreshold_ShouldHaveError() =>
        new DefineEvacuationZoneCommandValidator().TestValidate(ValidDefineZone() with { OccupancyThreshold = -1 })
            .ShouldHaveValidationErrorFor(c => c.OccupancyThreshold);

    // ---- RecordOccupancySnapshotCommandValidator ----------------------------------

    private static RecordOccupancySnapshotCommand ValidRecordOccupancy() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), 80, "Manual", TestUserId.Value);

    [Fact]
    public void RecordOccupancySnapshotCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new RecordOccupancySnapshotCommandValidator().TestValidate(ValidRecordOccupancy()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RecordOccupancySnapshotCommandValidator_WithNegativeHeadcount_ShouldHaveError() =>
        new RecordOccupancySnapshotCommandValidator().TestValidate(ValidRecordOccupancy() with { HeadcountEstimate = -1 })
            .ShouldHaveValidationErrorFor(c => c.HeadcountEstimate);

    [Fact]
    public void RecordOccupancySnapshotCommandValidator_WithBlankSource_ShouldHaveError() =>
        new RecordOccupancySnapshotCommandValidator().TestValidate(ValidRecordOccupancy() with { Source = "" })
            .ShouldHaveValidationErrorFor(c => c.Source);

    // ---- RunEgressSimulationCommandValidator --------------------------------------

    private static RunEgressSimulationCommand ValidRunSimulation() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), null, "v2026.1.0", "Manual", 300, 40, TestUserId.Value);

    [Fact]
    public void RunEgressSimulationCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new RunEgressSimulationCommandValidator().TestValidate(ValidRunSimulation()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void RunEgressSimulationCommandValidator_WithEmptyZoneId_ShouldHaveError() =>
        new RunEgressSimulationCommandValidator().TestValidate(ValidRunSimulation() with { ZoneId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.ZoneId);

    [Fact]
    public void RunEgressSimulationCommandValidator_WithBlankTriggerReason_ShouldHaveError() =>
        new RunEgressSimulationCommandValidator().TestValidate(ValidRunSimulation() with { TriggerReason = "" })
            .ShouldHaveValidationErrorFor(c => c.TriggerReason);

    [Fact]
    public void RunEgressSimulationCommandValidator_WithNegativeSimulatedTime_ShouldHaveError() =>
        new RunEgressSimulationCommandValidator().TestValidate(ValidRunSimulation() with { SimulatedEvacuationTimeSeconds = -1 })
            .ShouldHaveValidationErrorFor(c => c.SimulatedEvacuationTimeSeconds);

    // ---- LinkSimulationToDrillCommandValidator ------------------------------------

    private static LinkSimulationToDrillCommand ValidLinkToDrill() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void LinkSimulationToDrillCommandValidator_WithValidCommand_ShouldNotHaveErrors() =>
        new LinkSimulationToDrillCommandValidator().TestValidate(ValidLinkToDrill()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void LinkSimulationToDrillCommandValidator_WithEmptyDrillId_ShouldHaveError() =>
        new LinkSimulationToDrillCommandValidator().TestValidate(ValidLinkToDrill() with { DrillId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.DrillId);

    [Fact]
    public void LinkSimulationToDrillCommandValidator_WithEmptyRunId_ShouldHaveError() =>
        new LinkSimulationToDrillCommandValidator().TestValidate(ValidLinkToDrill() with { EgressSimulationRunId = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.EgressSimulationRunId);
}
