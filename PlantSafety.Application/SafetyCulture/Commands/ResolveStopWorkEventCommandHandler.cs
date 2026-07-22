// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ResolveStopWorkEventCommand. Pure orchestration: loads the StopWorkEvent aggregate, invokes its Resolve() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.Interfaces;
using PlantSafety.Domain.SafetyCulture.StopWorkEvent.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Handles <see cref="ResolveStopWorkEventCommand"/>.</summary>
public sealed class ResolveStopWorkEventCommandHandler : IRequestHandler<ResolveStopWorkEventCommand, Result<ResolveStopWorkEventResult>>
{
    private readonly IStopWorkEventRepository _repository;
    private readonly ILogger<ResolveStopWorkEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveStopWorkEventCommandHandler(IStopWorkEventRepository repository, ILogger<ResolveStopWorkEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ResolveStopWorkEventResult>> Handle(ResolveStopWorkEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = StopWorkEventId.FromGuid(request.StopWorkEventId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<ResolveStopWorkEventResult>.Failure(new ApplicationError(
                    "SafetyCulture.StopWorkEvent.NotFound",
                    "The requested StopWorkEvent record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Resolve(
                resolutionSummary: request.ResolutionSummary,
                wasCelebrated: request.WasCelebrated,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ResolveStopWorkEventResult>.Success(new ResolveStopWorkEventResult(
                aggregate.StopWorkEventId.Value,
                aggregate.Status.ToString(),
                aggregate.WasCelebrated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve StopWorkEvent {StopWorkEventId}", request.StopWorkEventId);

            return Result<ResolveStopWorkEventResult>.Failure(new ApplicationError(
                "SafetyCulture.StopWorkEvent.ResolveFailed",
                "Unable to resolve the StopWorkEvent at this time.",
                request.CorrelationId));
        }
    }
}
