// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for DeprecatePermitTemplateCommand.
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

/// <summary>Handles <see cref="DeprecatePermitTemplateCommand"/>.</summary>
public sealed class DeprecatePermitTemplateCommandHandler : IRequestHandler<DeprecatePermitTemplateCommand, Result<PermitTemplateActionResult>>
{
    private readonly IPermitTemplateRepository _repository;
    private readonly ILogger<DeprecatePermitTemplateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeprecatePermitTemplateCommandHandler(IPermitTemplateRepository repository, ILogger<DeprecatePermitTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitTemplateActionResult>> Handle(DeprecatePermitTemplateCommand request, CancellationToken cancellationToken)
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

            aggregate.Deprecate(
                reason: request.Reason,
                deprecatedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to deprecate PermitTemplate {PermitTemplateId}", request.PermitTemplateId);

            return Result<PermitTemplateActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitTemplate.DeprecateFailed",
                "Unable to deprecate the permit template at this time.",
                Guid.NewGuid()));
        }
    }
}
