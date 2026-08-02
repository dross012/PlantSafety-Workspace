// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           PublishCultureCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for PublishCultureCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="PublishCultureCommand"/>.</summary>
public sealed class PublishCultureCommandHandler : IRequestHandler<PublishCultureCommand, Result<CultureStatusResult>>
{
    private readonly ICultureRepository _repository;
    private readonly ILogger<PublishCultureCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishCultureCommandHandler(ICultureRepository repository, ILogger<PublishCultureCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CultureStatusResult>> Handle(PublishCultureCommand request, CancellationToken cancellationToken)
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

            var publishedBy = UserId.FromGuid(request.PublishedByUserId);

            aggregate.Publish(
                publishedBy: publishedBy,
                publishedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(publishedBy),
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
            _logger.LogError(ex, "Failed to publish Culture {CultureId}", request.CultureId);

            return Result<CultureStatusResult>.Failure(new ApplicationError(
                "Analytics.Culture.PublishFailed",
                "Unable to publish the culture record at this time.",
                Guid.NewGuid()));
        }
    }
}
