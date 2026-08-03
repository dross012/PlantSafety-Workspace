// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Per-EventType dead-letter analytics row, sourced from
//   [System].[usp_DeadLetterEvents_GetCountByEventType]. Suitable for a future DLQ dashboard
//   summary widget (severity distribution equivalent: dead-letter volume by event type).
// SchemaVersion: 1

using System;

namespace PlantSafety.Application.Messaging.Models;

/// <summary>Dead-letter analytics summary for a single <c>EventType</c>, over a lookback window.</summary>
/// <param name="EventType">The canonical event-type discriminator being summarised.</param>
/// <param name="DeadLetterCount">Total dead-lettered rows for this event type within the lookback window.</param>
/// <param name="OldestFaultedAtUtc">Earliest <c>FaultedAt</c> within the window.</param>
/// <param name="NewestFaultedAtUtc">Most recent <c>FaultedAt</c> within the window.</param>
/// <param name="OldestAgeHours">Age, in hours, of the oldest dead-lettered row in this group.</param>
/// <param name="AverageRetryCount">Average number of delivery attempts recorded before dead-lettering.</param>
public sealed record DeadLetterEventsSummaryDto(
    string EventType,
    int DeadLetterCount,
    DateTime OldestFaultedAtUtc,
    DateTime NewestFaultedAtUtc,
    int OldestAgeHours,
    double AverageRetryCount);
