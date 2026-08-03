// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           RebuildWorkOrderTimelineQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.4 Replay/Rebuild Across Systems, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Handles <see cref="RebuildWorkOrderTimelineQuery"/> by replaying the WorkOrder's
//                  raw domain-event history straight from <c>[System].[OutboxMessages]</c> (via
//                  <see cref="IWorkOrderEventReplayRepository"/>) and folding each event, in strict
//                  chronological order, into a reconstructed <see cref="WorkOrderRebuiltStateDto"/> --
//                  the same "merge heterogeneous chronological entries into one coherent state"
//                  pattern <c>AuditTimelineCompositionService</c> already established for the Phase 5
//                  audit timeline (that Domain-layer type itself lives in the Auditing module and
//                  cannot be referenced directly here without violating Handbook Part IX's "no
//                  cross-module Domain references" rule, so the ordering/composition pattern is
//                  reapplied locally rather than reusing the type). Pure, deterministic, and
//                  side-effect free: replay never mutates the underlying Outbox rows. Additive-only
//                  evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.WorkOrders.Models;
using PlantSafety.Application.Operational.WorkOrders.Queries;
using PlantSafety.Application.Operational.WorkOrders.Repositories;

namespace PlantSafety.Application.Operational.WorkOrders.Handlers;

/// <summary>Handles <see cref="RebuildWorkOrderTimelineQuery"/> by replaying Outbox-recorded WorkOrder events.</summary>
public sealed class RebuildWorkOrderTimelineQueryHandler(
    IWorkOrderEventReplayRepository replayRepository,
    ILogger<RebuildWorkOrderTimelineQueryHandler> logger)
    : IRequestHandler<RebuildWorkOrderTimelineQuery, Result<WorkOrderRebuiltStateDto>>
{
    /// <inheritdoc/>
    public async Task<Result<WorkOrderRebuiltStateDto>> Handle(RebuildWorkOrderTimelineQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var rawEvents = await replayRepository.GetRawEventsAsync(request.TenantId, request.WorkOrderId, cancellationToken);

            var replayable = rawEvents
                .Where(e => request.AsOfUtc is null || e.OccurredAtUtc <= request.AsOfUtc.Value)
                .OrderBy(e => e.OccurredAtUtc)
                .ToList();

            if (replayable.Count == 0)
            {
                return Result<WorkOrderRebuiltStateDto>.Failure(new ApplicationError(
                    "Operational.WorkOrders.RebuildNoEvents",
                    "No Outbox-recorded event history exists for this WorkOrder (or none occurred before the requested point in time).",
                    Guid.NewGuid()));
            }

            var state = Replay(request.WorkOrderId, replayable);
            return Result<WorkOrderRebuiltStateDto>.Success(state);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rebuild WorkOrder {WorkOrderId} timeline from Outbox history", request.WorkOrderId);
            return Result<WorkOrderRebuiltStateDto>.Failure(new ApplicationError(
                "Operational.WorkOrders.RebuildFailed", "Unable to rebuild work order timeline at this time.", Guid.NewGuid()));
        }
    }

    /// <summary>
    /// Pure fold over the chronologically-ordered raw event stream, reconstructing the WorkOrder's
    /// current (or point-in-time) state. Unknown/future EventTypes are skipped, not thrown on --
    /// matching this platform's additive-only-evolution replay-compatibility rule (Handbook Part XVI).
    /// </summary>
    private static WorkOrderRebuiltStateDto Replay(Guid workOrderId, IReadOnlyList<WorkOrderReplayEventRecord> events)
    {
        string status = "Unknown";
        DateTimeOffset? createdAtUtc = null;
        Guid? lastAssignedTechnicianId = null;
        string? completionNotes = null;

        foreach (var evt in events)
        {
            using var payload = JsonDocument.Parse(evt.PayloadJson);
            var root = payload.RootElement;

            switch (evt.EventType)
            {
                case "WorkOrderCreated":
                    status = "Created";
                    createdAtUtc = evt.OccurredAtUtc;
                    break;

                case "WorkOrderAssigned":
                    status = "Assigned";
                    if (TryGetGuidProperty(root, "technicianId", out var technicianId))
                        lastAssignedTechnicianId = technicianId;
                    break;

                case "WorkOrderStatusChanged":
                    if (root.TryGetProperty("newStatus", out var newStatusElement) && newStatusElement.ValueKind == JsonValueKind.String)
                        status = newStatusElement.GetString() ?? status;
                    break;

                case "WorkOrderCompleted":
                    status = "Completed";
                    if (root.TryGetProperty("completionNotes", out var notesElement) && notesElement.ValueKind == JsonValueKind.String)
                        completionNotes = notesElement.GetString();
                    break;

                default:
                    // Additive-only evolution: a future WorkOrder event type this replay handler does
                    // not yet know how to fold is skipped, never thrown on -- replay compatibility is
                    // preserved for older AND newer envelopes alike.
                    break;
            }
        }

        return new WorkOrderRebuiltStateDto
        {
            WorkOrderId = workOrderId,
            AsOfUtc = events[^1].OccurredAtUtc,
            Status = status,
            CreatedAtUtc = createdAtUtc,
            LastAssignedTechnicianId = lastAssignedTechnicianId,
            CompletionNotes = completionNotes,
            ReplayedEvents = events
                .Select(e => new WorkOrderRebuiltTimelineEntryDto { EventType = e.EventType, OccurredAtUtc = e.OccurredAtUtc })
                .ToList(),
        };
    }

    private static bool TryGetGuidProperty(JsonElement root, string propertyName, out Guid value)
    {
        value = Guid.Empty;

        if (!root.TryGetProperty(propertyName, out var element))
            return false;

        // This codebase's validating-value-object convention (see
        // ValidatingValueObjectJsonConverterFactory) serializes strong-typed ids as a JSON object
        // with a camelCase "value" property (e.g. {"value":"<guid>"}) rather than a bare string --
        // handle both shapes defensively since not every id-shaped field is guaranteed to be a
        // wrapped value object at every call site.
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("value", out var nested))
            return nested.TryGetGuid(out value);

        return element.ValueKind == JsonValueKind.String && element.TryGetGuid(out value);
    }
}
