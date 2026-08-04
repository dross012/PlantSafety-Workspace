// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CloseCyberPhysicalIncidentCommand. Progresses the incident through
//   Investigating/Contained/Resolved as needed before closing, so a caller can close directly from
//   Open per the lenient transition map, and computes ResponseTimeMinutes (KPI) via the aggregate.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;

/// <summary>Handles <see cref="CloseCyberPhysicalIncidentCommand"/>.</summary>
public sealed class CloseCyberPhysicalIncidentCommandHandler : IRequestHandler<CloseCyberPhysicalIncidentCommand, Result<CloseCyberPhysicalIncidentResult>>
{
    private readonly ICyberPhysicalIncidentRepository _repository;
    private readonly ILogger<CloseCyberPhysicalIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseCyberPhysicalIncidentCommandHandler(ICyberPhysicalIncidentRepository repository, ILogger<CloseCyberPhysicalIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CloseCyberPhysicalIncidentResult>> Handle(CloseCyberPhysicalIncidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actorId = UserId.FromGuid(request.ActorId);
            var id = CyberPhysicalIncidentId.FromGuid(request.CyberPhysicalIncidentId);

            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<CloseCyberPhysicalIncidentResult>.Failure(new ApplicationError(
                    "CyberPhysicalSecurity.CyberPhysicalIncident.NotFound",
                    "The requested cyber-physical incident could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);

            aggregate.Close(UtcTimestamp.Now(), PerformedByActor.User(actorId), correlationId, causationId);

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CloseCyberPhysicalIncidentResult>.Success(new CloseCyberPhysicalIncidentResult(
                aggregate.CyberPhysicalIncidentId.Value,
                aggregate.Status.ToString(),
                aggregate.ResponseTimeMinutes ?? 0));
        }
        catch (CyberPhysicalIncidentDomainException ex)
        {
            return Result<CloseCyberPhysicalIncidentResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.CyberPhysicalIncident.InvalidTransition", ex.Message, Guid.NewGuid(), ErrorSeverity.Domain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close CyberPhysicalIncident {CyberPhysicalIncidentId}", request.CyberPhysicalIncidentId);

            return Result<CloseCyberPhysicalIncidentResult>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.CyberPhysicalIncident.CloseFailed",
                "Unable to close the cyber-physical incident at this time.",
                Guid.NewGuid()));
        }
    }
}
