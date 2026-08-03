// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query for a per-EventType dead-letter analytics summary, suitable for a future
//   DLQ dashboard page (PlantSafety.UI\Modules pattern -- not built here per this task's scope
//   note: only ship the query when the UI page itself is not a trivial ~30-minute slot-in).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Models;

namespace PlantSafety.Application.Messaging.Queries;

/// <summary>Retrieves dead-letter counts/ages grouped by EventType over a lookback window.</summary>
/// <param name="TenantId">Optional exact match on TenantId. Null returns all tenants.</param>
/// <param name="LookbackDays">How many days back to include. Defaults to 7.</param>
public sealed record GetDeadLetterEventsSummaryQuery(
    string? TenantId = null,
    int LookbackDays = 7)
    : IRequest<Result<IReadOnlyList<DeadLetterEventsSummaryDto>>>;
