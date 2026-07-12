// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateRootCauseAnalysisCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.RootCauseAnalysis.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;

/// <summary>Handles <see cref="CreateRootCauseAnalysisCommand"/>. Returns the new RcaId.</summary>
public sealed class CreateRcaCommandHandler : ICommandHandler<CreateRootCauseAnalysisCommand, Guid>
{
    private readonly IRcaRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateRcaCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRcaCommandHandler(
        IRcaRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateRcaCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateRootCauseAnalysisCommand request, CancellationToken cancellationToken)
    {
        var rcaId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            rcaId: rcaId,
            tenantId: request.TenantId,
            linkedEventId: request.LinkedEventId,
            linkedEventType: request.LinkedEventType,
            methodology: request.Methodology,
            rootCauses: request.RootCauses,
            correctiveActions: request.CorrectiveActions,
            leadInvestigatorId: request.LeadInvestigatorId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "RCA {RcaId} created for LinkedEvent {LinkedEventId} Tenant {TenantId} by Actor {ActorId}",
            rcaId, request.LinkedEventId, request.TenantId, request.ActorId);

        return rcaId;
    }
}
