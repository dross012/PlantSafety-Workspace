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
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Application.Enterprise.EventMesh.Commands;

/// <summary>Handles <see cref="IngestEnterpriseInboundEventCommand"/>.</summary>
public sealed class IngestEnterpriseInboundEventCommandHandler(
    IEnumerable<IEnterpriseInboundEventTranslator> translators,
    EventUpgradePipeline upgradePipeline,
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

        var dispatchResult = await translator.TranslateAndDispatchAsync(envelope, cancellationToken);

        if (dispatchResult.IsFailure)
        {
            logger.LogWarning(
                "EnterpriseEventMesh: inbound translation failed for EventType={EventType} (EventId={EventId}): [{ErrorCode}] {Message}",
                envelope.EventType, envelope.EventId, dispatchResult.Error!.Code.Value, dispatchResult.Error.Message);

            return Result<EnterpriseInboundIngestResult>.Failure(new ApplicationError(
                dispatchResult.Error.Code.Value, dispatchResult.Error.Message, request.CorrelationId));
        }

        return Result<EnterpriseInboundIngestResult>.Success(
            new EnterpriseInboundIngestResult(envelope.EventId, envelope.EventType, Handled: true));
    }
}
