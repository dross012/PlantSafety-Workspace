// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to retrieve recent dead-lettered events, newest first, paged.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Models;

namespace PlantSafety.Application.Messaging.Queries;

/// <summary>Retrieves recent dead-lettered events, most recently faulted first.</summary>
/// <param name="TenantId">Optional exact match on TenantId. Null returns all tenants.</param>
/// <param name="EventType">Optional exact match on EventType. Null returns all event types.</param>
/// <param name="IncludeReplayed">When <see langword="false"/>, excludes already-replayed rows. Defaults to <see langword="true"/>.</param>
/// <param name="Top">Page size. Defaults to 50.</param>
/// <param name="Skip">Rows to skip for pagination. Defaults to 0.</param>
public sealed record GetDeadLetterEventsRecentQuery(
    string? TenantId = null,
    string? EventType = null,
    bool IncludeReplayed = true,
    int Top = 50,
    int Skip = 0)
    : IRequest<Result<IReadOnlyList<DeadLetterEventDto>>>;
