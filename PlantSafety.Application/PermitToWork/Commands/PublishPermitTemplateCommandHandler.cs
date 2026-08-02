// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for PublishPermitTemplateCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PermitToWork.PermitTemplate.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitTemplate.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Handles <see cref="PublishPermitTemplateCommand"/>.</summary>
public sealed class PublishPermitTemplateCommandHandler : IRequestHandler<PublishPermitTemplateCommand, Result<PermitTemplateActionResult>>
{
    private readonly IPermitTemplateRepository _repository;
    private readonly ILogger<PublishPermitTemplateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PublishPermitTemplateCommandHandler(IPermitTemplateRepository repository, ILogger<PublishPermitTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitTemplateActionResult>> Handle(PublishPermitTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitTemplateId.FromGuid(request.PermitTemplateId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitTemplateActionResult>.Failure(new ApplicationError(
                    "PermitToWork.PermitTemplate.NotFound",
                    "The requested permit template could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Publish(
                publishedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PermitTemplateActionResult>.Success(new PermitTemplateActionResult(
                aggregate.PermitTemplateId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PermitTemplate {PermitTemplateId}", request.PermitTemplateId);

            return Result<PermitTemplateActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitTemplate.PublishFailed",
                "Unable to publish the permit template at this time.",
                Guid.NewGuid()));
        }
    }
}
