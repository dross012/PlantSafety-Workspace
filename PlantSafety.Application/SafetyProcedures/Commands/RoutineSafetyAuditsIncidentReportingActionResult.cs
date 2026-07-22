// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for RoutineSafetyAuditsIncidentReporting lifecycle commands ("SafetyProcedures.RoutineSafetyAuditsIncidentReporting").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful RoutineSafetyAuditsIncidentReporting lifecycle command execution.</summary>
/// <param name="RoutineSafetyAuditsIncidentReportingId">Identity of the affected RoutineSafetyAuditsIncidentReporting aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record RoutineSafetyAuditsIncidentReportingActionResult(
    Guid RoutineSafetyAuditsIncidentReportingId,
    string Status);