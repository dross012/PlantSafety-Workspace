// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           ActivateDigitalTwinCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateDigitalTwinCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.Repositories;
using PlantSafety.Domain.DigitalTwin.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Handles <see cref="ActivateDigitalTwinCommand"/>.</summary>
public sealed class ActivateDigitalTwinCommandHandler : IRequestHandler<ActivateDigitalTwinCommand, Result<DigitalTwinStatusResult>>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<ActivateDigitalTwinCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateDigitalTwinCommandHandler(IDigitalTwinRepository repository, ILogger<ActivateDigitalTwinCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DigitalTwinStatusResult>> Handle(ActivateDigitalTwinCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DigitalTwinId.FromGuid(request.DigitalTwinId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DigitalTwinStatusResult>.Failure(new ApplicationError(
                    "DigitalTwin.NotFound",
                    "The requested digital-twin record could not be found.",
                    request.DigitalTwinId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Activate(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DigitalTwinStatusResult>.Success(new DigitalTwinStatusResult(
                aggregate.DigitalTwinId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate DigitalTwin {DigitalTwinId}", request.DigitalTwinId);

            return Result<DigitalTwinStatusResult>.Failure(new ApplicationError(
                "DigitalTwin.ActivateFailed",
                "Unable to activate the digital-twin record at this time.",
                Guid.NewGuid()));
        }
    }
}
