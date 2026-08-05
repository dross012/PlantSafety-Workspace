// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ResolveCommunityConcernCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="ResolveCommunityConcernCommand"/>.</summary>
public sealed class ResolveCommunityConcernCommandHandler : IRequestHandler<ResolveCommunityConcernCommand, Result<CommunityConcernActionResult>>
{
    private readonly ICommunityConcernRepository _repository;
    private readonly ILogger<ResolveCommunityConcernCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveCommunityConcernCommandHandler(ICommunityConcernRepository repository, ILogger<ResolveCommunityConcernCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityConcernActionResult>> Handle(ResolveCommunityConcernCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CommunityConcernId.FromGuid(request.CommunityConcernId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CommunityConcernActionResult>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityConcern.NotFound",
                    "The requested CommunityConcern record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Resolve(
                request.ResolutionNotes,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CommunityConcernActionResult>.Success(new CommunityConcernActionResult(
                aggregate.CommunityConcernId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve CommunityConcern {CommunityConcernId}", request.CommunityConcernId);

            return Result<CommunityConcernActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityConcern.ResolveFailed",
                "Unable to resolve the CommunityConcern record at this time.",
                Guid.NewGuid()));
        }
    }
}
