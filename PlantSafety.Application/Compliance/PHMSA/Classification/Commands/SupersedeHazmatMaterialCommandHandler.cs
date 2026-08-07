// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           SupersedeHazmatMaterialCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for SupersedeHazmatMaterialCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Commands;

/// <summary>Handles <see cref="SupersedeHazmatMaterialCommand"/>.</summary>
public sealed class SupersedeHazmatMaterialCommandHandler : IRequestHandler<SupersedeHazmatMaterialCommand, Result<HazmatMaterialStatusResult>>
{
    private readonly IHazmatMaterialRepository _repository;
    private readonly ILogger<SupersedeHazmatMaterialCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SupersedeHazmatMaterialCommandHandler(IHazmatMaterialRepository repository, ILogger<SupersedeHazmatMaterialCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazmatMaterialStatusResult>> Handle(SupersedeHazmatMaterialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HazmatMaterialId.FromGuid(request.HazmatMaterialId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HazmatMaterialStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Classification.NotFound",
                    "The requested hazmat classification could not be found.",
                    request.HazmatMaterialId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Supersede(
                supersededByMaterialId: HazmatMaterialId.FromGuid(request.SupersededByMaterialId),
                supersededAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<HazmatMaterialStatusResult>.Success(new HazmatMaterialStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to supersede HazmatMaterial {HazmatMaterialId}", request.HazmatMaterialId);

            return Result<HazmatMaterialStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Classification.SupersedeFailed",
                "Unable to supersede the hazmat classification at this time.",
                Guid.NewGuid()));
        }
    }
}
