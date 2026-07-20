// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Monitoring
//  Layer:          Application
//  File:           DeadlockEventDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single xml_deadlock_report event, parsed from the built-in
//                  "system_health" Extended Events session's ring_buffer target.
//                  Phase13-Monitoring\131-extended-events-sessions.sql only defines
//                  PlantSafety_ErrorCapture (error_reported, severity >= 16) and
//                  PlantSafety_LongRunningQueries (rpc/batch completions > 1000ms) — it
//                  does NOT add a custom deadlock-capturing session. Deadlock capture on
//                  this SQL Server Express instance is therefore provided by the built-in,
//                  always-on "system_health" session (the standard SQL Server default),
//                  confirmed by the absence of any custom session in 131 that subscribes
//                  to sqlserver.xml_deadlock_report. Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Monitoring.Models;

/// <summary>A single deadlock event parsed from the "system_health" XE session's ring buffer.</summary>
/// <param name="EventTimeUtc">The event's @timestamp attribute.</param>
/// <param name="VictimProcessId">The XE process id of the process chosen as deadlock victim.</param>
/// <param name="ParticipantCount">Number of processes involved in the deadlock cycle.</param>
/// <param name="ResourceSummary">Human-readable summary of the contended resource(s) (lock type / object name).</param>
/// <param name="RawXml">The full &lt;deadlock&gt; XML fragment, for drill-down/export.</param>
public sealed record DeadlockEventDto(
    DateTime EventTimeUtc,
    string? VictimProcessId,
    int ParticipantCount,
    string ResourceSummary,
    string RawXml);
