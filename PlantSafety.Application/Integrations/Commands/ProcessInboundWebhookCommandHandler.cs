// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ProcessInboundWebhookCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Integrations.Commands;

/// <summary>Handles <see cref="ProcessInboundWebhookCommand"/>.</summary>
public sealed class ProcessInboundWebhookCommandHandler : IRequestHandler<ProcessInboundWebhookCommand, Result<ProcessInboundWebhookResult>>
{
    private readonly IInboundWebhookRepository _repository;
    private readonly ILogger<ProcessInboundWebhookCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ProcessInboundWebhookCommandHandler(IInboundWebhookRepository repository, ILogger<ProcessInboundWebhookCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessInboundWebhookResult>> Handle(ProcessInboundWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.RecordEventAsync(
                request.EventId,
                request.EventType,
                request.Source,
                request.TenantId,
                request.SiteId,
                request.OccurredAt,
                request.RawPayload,
                cancellationToken);

            return Result<ProcessInboundWebhookResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process inbound webhook. EventId {EventId} Source {Source}", request.EventId, request.Source);

            return Result<ProcessInboundWebhookResult>.Failure(new ApplicationError(
                "Integrations.Webhooks.ProcessingFailed",
                "Unable to process the inbound webhook event at this time.",
                Guid.NewGuid()));
        }
    }
}
