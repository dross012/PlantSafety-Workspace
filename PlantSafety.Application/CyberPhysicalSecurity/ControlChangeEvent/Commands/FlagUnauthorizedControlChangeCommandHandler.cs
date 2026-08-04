// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for FlagUnauthorizedControlChangeCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;

/// <summary>Handles <see cref="FlagUnauthorizedControlChangeCommand"/>.</summary>
public sealed class FlagUnauthorizedControlChangeCommandHandler : IRequestHandler<FlagUnauthorizedControlChangeCommand, Result<FlagUnauthorizedControlChangeResult>>
{
    private readonly IControlChangeEventRepository _repository;
    private readonly ILogger<FlagUnauthorizedControlChangeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagUnauthorizedControlChangeCommandHandler(IControlChangeEventRepository repository, ILogger<FlagUnauthorizedControlChangeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FlagUnauthorizedControlChangeResult>> Handle(FlagUnauthorizedControlChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);
            var id = ControlChangeEventId.FromGuid(request.ControlChangeEventId);

            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<FlagUnauthorizedControlChangeResult>.Failure(new ApplicationError(
                    "CyberPhysicalSecurity.ControlChangeEvent.NotFound",
                    "The requested control-change event could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            aggregate.FlagUnauthorized(
                detectedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<FlagUnauthorizedControlChangeResult>.Success(new FlagUnauthorizedControlChangeResult(
                aggregate.ControlChangeEventId.Value,
                aggregate.AuthorizationStatus.ToString()));
        }
        catch (ControlChangeEventDomainException ex)
        {
            return Result<FlagUnauthorizedControlChangeResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.ControlChangeEvent.InvalidOperation", ex.Message, Guid.NewGuid(), ErrorSeverity.Domain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flag ControlChangeEvent {ControlChangeEventId} unauthorized", request.ControlChangeEventId);

            return Result<FlagUnauthorizedControlChangeResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.ControlChangeEvent.FlagFailed",
                "Unable to flag the control-change event at this time.",
                Guid.NewGuid()));
        }
    }
}
