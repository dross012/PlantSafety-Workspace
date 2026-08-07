// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           RetirePackagingCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for RetirePackagingCommand. Mirrors SupersedePackagingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Handles <see cref="RetirePackagingCommand"/>.</summary>
public sealed class RetirePackagingCommandHandler : IRequestHandler<RetirePackagingCommand, Result<PackagingStatusResult>>
{
    private readonly IPackagingSpecificationRepository _repository;
    private readonly ILogger<RetirePackagingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetirePackagingCommandHandler(IPackagingSpecificationRepository repository, ILogger<RetirePackagingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PackagingStatusResult>> Handle(RetirePackagingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PackagingSpecificationId.FromGuid(request.PackagingSpecificationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PackagingStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Packaging.NotFound",
                    "The requested packaging specification could not be found.",
                    request.PackagingSpecificationId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Retire(
                retiredAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PackagingStatusResult>.Success(new PackagingStatusResult(
                aggregate.Id.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire PackagingSpecification {PackagingSpecificationId}", request.PackagingSpecificationId);

            return Result<PackagingStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Packaging.RetireFailed",
                "Unable to retire the packaging specification at this time.",
                Guid.NewGuid()));
        }
    }
}
