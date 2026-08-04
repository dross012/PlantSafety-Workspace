// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Application
//  File:           IngestEnterpriseInboundEventCommandHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2/§3.3, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Handles <see cref="IngestEnterpriseInboundEventCommand"/>: upgrades the received
//                  envelope to its latest registered schema version (Phase 6 §3.3 replay/version
//                  compatibility), then resolves the first registered
//                  <see cref="IEnterpriseInboundEventTranslator"/> that recognises the envelope's
//                  EventType and lets it dispatch the target module's own public command. An
//                  unrecognised EventType is gracefully ignored (logged, not failed) -- an external
//                  system emitting an EventType this platform version does not yet understand must
//                  never break the inbound adapter (additive-only evolution / Handbook Part XVI).
//                  Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 9 §1 Reliability Hardening): a genuinely failed inbound
//                  translation/dispatch (dispatchResult.IsFailure) or an unhandled exception thrown
//                  by a translator (e.g. a malformed/unexpected payload shape) previously only
//                  produced a log line -- the external system's caller received a 400 response, but
//                  no queryable trail of the failure was ever persisted anywhere, distinct from the
//                  internal MassTransit fault pipeline's [System].[DeadLetterEvents] capture. Both
//                  paths now also record a diagnostic row via
//                  IDeadLetterEventRepository.RecordInboundEventMeshFailureAsync, reusing the same
//                  table/shape as the internal DLQ so a poisoned/permanently-failing inbound ERP/MES/
//                  CMMS/HR/Vendor payload leaves the same operator-visible trail an internal poison
//                  message does. The unhandled-exception path is caught here (not left to bubble to
//                  ASP.NET's generic exception middleware) and converted to a Result failure --
//                  this inbound adapter must never crash from an external system's malformed data.
//                  Additive-only evolution.
// ============================================================================
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Repositories;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Application.Enterprise.EventMesh.Commands;

/// <summary>Handles <see cref="IngestEnterpriseInboundEventCommand"/>.</summary>
public sealed class IngestEnterpriseInboundEventCommandHandler(
    IEnumerable<IEnterpriseInboundEventTranslator> translators,
    EventUpgradePipeline upgradePipeline,
    IDeadLetterEventRepository deadLetterEventRepository,
    ILogger<IngestEnterpriseInboundEventCommandHandler> logger)
    : IRequestHandler<IngestEnterpriseInboundEventCommand, Result<EnterpriseInboundIngestResult>>
{
    /// <inheritdoc/>
    public async Task<Result<EnterpriseInboundIngestResult>> Handle(
        IngestEnterpriseInboundEventCommand request, CancellationToken cancellationToken)
    {
        var envelope = upgradePipeline.UpgradeToLatest(request.Envelope);

        var translator = translators.FirstOrDefault(t => t.CanHandle(envelope));
        if (translator is null)
        {
            logger.LogInformation(
                "EnterpriseEventMesh: no registered inbound translator for EventType={EventType} (EventId={EventId}, SourceSystem={SourceSystem}); ignored gracefully.",
                envelope.EventType, envelope.EventId, envelope.SourceSystem);

            return Result<EnterpriseInboundIngestResult>.Success(
                new EnterpriseInboundIngestResult(envelope.EventId, envelope.EventType, Handled: false));
        }

        try
        {
            var dispatchResult = await translator.TranslateAndDispatchAsync(envelope, cancellationToken);

            if (dispatchResult.IsFailure)
            {
                logger.LogWarning(
                    "EnterpriseEventMesh: inbound translation failed for EventType={EventType} (EventId={EventId}): [{ErrorCode}] {Message}",
                    envelope.EventType, envelope.EventId, dispatchResult.Error!.Code.Value, dispatchResult.Error.Message);

                await RecordDeadLetterAsync(envelope, dispatchResult.Error.Message, exceptionDetail: null, cancellationToken);

                return Result<EnterpriseInboundIngestResult>.Failure(new ApplicationError(
                    dispatchResult.Error.Code.Value, dispatchResult.Error.Message, request.CorrelationId));
            }

            return Result<EnterpriseInboundIngestResult>.Success(
                new EnterpriseInboundIngestResult(envelope.EventId, envelope.EventType, Handled: true));
        }
        catch (Exception ex)
        {
            // An inbound external-system adapter must never crash the API pipeline from a
            // malformed/unexpected payload -- surfaced loudly (logged + dead-lettered), converted
            // to a Result failure rather than left to propagate.
            logger.LogError(ex,
                "EnterpriseEventMesh: inbound translator threw for EventType={EventType} (EventId={EventId}, SourceSystem={SourceSystem}).",
                envelope.EventType, envelope.EventId, envelope.SourceSystem);

            await RecordDeadLetterAsync(envelope, ex.Message, ex.ToString(), cancellationToken);

            return Result<EnterpriseInboundIngestResult>.Failure(new ApplicationError(
                "enterprise.eventmesh.inboundtranslationexception", ex.Message, request.CorrelationId));
        }
    }

    private async Task RecordDeadLetterAsync(
        EnterpriseEventEnvelope envelope, string faultReason, string? exceptionDetail, CancellationToken cancellationToken)
    {
        var payload = SerializeEnvelope(envelope);

        await deadLetterEventRepository.RecordInboundEventMeshFailureAsync(
            eventType: envelope.EventType,
            schemaVersion: envelope.SchemaVersion.ToString(),
            tenantId: envelope.TenantId == Guid.Empty ? null : envelope.TenantId.ToString(),
            payload: payload,
            faultReason: faultReason,
            exceptionDetail: exceptionDetail,
            correlationId: envelope.CorrelationId,
            causationId: envelope.CausationId,
            sourceSystem: envelope.SourceSystem.ToString(),
            cancellationToken: cancellationToken);
    }

    private static string? SerializeEnvelope(EnterpriseEventEnvelope envelope)
    {
        try
        {
            return JsonSerializer.Serialize(envelope);
        }
        catch
        {
            // Losing the diagnostic payload is preferable to losing the entire dead-letter row --
            // mirrors DeadLetterEventPersister.SerializePayload's identical fallback reasoning.
            return null;
        }
    }
}
