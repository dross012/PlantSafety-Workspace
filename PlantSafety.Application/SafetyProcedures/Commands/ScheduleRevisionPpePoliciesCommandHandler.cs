// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleRevisionPpePoliciesCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.PpePolicies.Interfaces;
using PlantSafety.Domain.SafetyProcedures.PpePolicies.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="ScheduleRevisionPpePoliciesCommand"/>.</summary>
public sealed class ScheduleRevisionPpePoliciesCommandHandler : IRequestHandler<ScheduleRevisionPpePoliciesCommand, Result<PpePoliciesActionResult>>
{
    private readonly IPpePoliciesRepository _repository;
    private readonly ILogger<ScheduleRevisionPpePoliciesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleRevisionPpePoliciesCommandHandler(IPpePoliciesRepository repository, ILogger<ScheduleRevisionPpePoliciesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpePoliciesActionResult>> Handle(ScheduleRevisionPpePoliciesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PpePoliciesId.FromGuid(request.PpePoliciesId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PpePoliciesActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.PpePolicies.NotFound",
                    "The requested PpePolicies record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ScheduleRevision(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PpePoliciesActionResult>.Success(new PpePoliciesActionResult(
                aggregate.PpePoliciesId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ScheduleRevision for PpePolicies {PpePoliciesId}", request.PpePoliciesId);

            return Result<PpePoliciesActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.PpePolicies.ScheduleRevisionFailed",
                "Unable to ScheduleRevision the PpePolicies record at this time.",
                Guid.NewGuid()));
        }
    }
}