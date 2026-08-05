// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportIncidents
//  Layer:          Tests (Unit)
//  File:           TransportIncidentCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the FluentValidation validators backing all TransportIncident
//                  commands: Record, BeginInvestigation, Close.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.Validation;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportIncidents;

public sealed class TransportIncidentCommandValidatorTests : UnitTestBase
{
    // ---- RecordTransportIncidentCommandValidator --------------------------------------

    private static RecordTransportIncidentCommand ValidRecordCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
        Guid.NewGuid(), Guid.NewGuid(), "Collision", "Moderate", "Loading dock 4", TestUserId.Value);

    [Fact]
    public void Record_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new RecordTransportIncidentCommandValidator().TestValidate(ValidRecordCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Record_WithEmptyIncidentType_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { IncidentType = "" };
        var result = new RecordTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.IncidentType);
    }

    [Fact]
    public void Record_WithEmptySeverity_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { Severity = "" };
        var result = new RecordTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Severity);
    }

    [Fact]
    public void Record_WithTooLongLocation_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { Location = new string('x', 301) };
        var result = new RecordTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }

    [Fact]
    public void Record_WithEmptyDriverUserId_ShouldHaveError()
    {
        var command = ValidRecordCommand() with { DriverUserId = Guid.Empty };
        var result = new RecordTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DriverUserId);
    }

    // ---- BeginInvestigationTransportIncidentCommandValidator --------------------------

    private static BeginInvestigationTransportIncidentCommand ValidBeginInvestigationCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void BeginInvestigation_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new BeginInvestigationTransportIncidentCommandValidator().TestValidate(ValidBeginInvestigationCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void BeginInvestigation_WithEmptyTransportIncidentId_ShouldHaveError()
    {
        var command = ValidBeginInvestigationCommand() with { TransportIncidentId = Guid.Empty };
        var result = new BeginInvestigationTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TransportIncidentId);
    }

    // ---- CloseTransportIncidentCommandValidator ----------------------------------------

    private static CloseTransportIncidentCommand ValidCloseCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void Close_WithValidCommand_ShouldNotHaveErrors()
    {
        var result = new CloseTransportIncidentCommandValidator().TestValidate(ValidCloseCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Close_WithEmptyPerformedByUserId_ShouldHaveError()
    {
        var command = ValidCloseCommand() with { PerformedByUserId = Guid.Empty };
        var result = new CloseTransportIncidentCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
