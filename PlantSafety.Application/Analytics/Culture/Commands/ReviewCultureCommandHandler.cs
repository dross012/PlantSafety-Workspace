// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           ReviewCultureCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ReviewCultureCommand. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException
//                  is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Interfaces.Persistence;
using PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Culture.Commands;

/// <summary>Handles <see cref="ReviewCultureCommand"/>.</summary>
public sealed class ReviewCultureCommandHandler : IRequestHandler<ReviewCultureCommand, Result<CultureStatusResult>>
{
    private readonly ICultureRepository _repository;
    private readonly ILogger<ReviewCultureCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReviewCultureCommandHandler(ICultureRepository repository, ILogger<ReviewCultureCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CultureStatusResult>> Handle(ReviewCultureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CultureId.FromGuid(request.CultureId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<CultureStatusResult>.Failure(new ApplicationError(
                    "Analytics.Culture.NotFound",
                    "The requested culture record could not be found.",
                    request.CultureId,
                    ErrorSeverity.NotFound));
            }

            var reviewedBy = UserId.FromGuid(request.ReviewedByUserId);

            aggregate.Review(
                reviewedBy: reviewedBy,
                reviewedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(reviewedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<CultureStatusResult>.Success(new CultureStatusResult(
                aggregate.CultureId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to review Culture {CultureId}", request.CultureId);

            return Result<CultureStatusResult>.Failure(new ApplicationError(
                "Analytics.Culture.ReviewFailed",
                "Unable to review the culture record at this time.",
                Guid.NewGuid()));
        }
    }
}
