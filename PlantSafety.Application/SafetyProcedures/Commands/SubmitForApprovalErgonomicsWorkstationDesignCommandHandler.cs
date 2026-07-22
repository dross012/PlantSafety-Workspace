// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SubmitForApprovalErgonomicsWorkstationDesignCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Interfaces;
using PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="SubmitForApprovalErgonomicsWorkstationDesignCommand"/>.</summary>
public sealed class SubmitForApprovalErgonomicsWorkstationDesignCommandHandler : IRequestHandler<SubmitForApprovalErgonomicsWorkstationDesignCommand, Result<ErgonomicsWorkstationDesignActionResult>>
{
    private readonly IErgonomicsWorkstationDesignRepository _repository;
    private readonly ILogger<SubmitForApprovalErgonomicsWorkstationDesignCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForApprovalErgonomicsWorkstationDesignCommandHandler(IErgonomicsWorkstationDesignRepository repository, ILogger<SubmitForApprovalErgonomicsWorkstationDesignCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsWorkstationDesignActionResult>> Handle(SubmitForApprovalErgonomicsWorkstationDesignCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ErgonomicsWorkstationDesignId.FromGuid(request.ErgonomicsWorkstationDesignId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<ErgonomicsWorkstationDesignActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.ErgonomicsWorkstationDesign.NotFound",
                    "The requested ErgonomicsWorkstationDesign record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.SubmitForApproval(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ErgonomicsWorkstationDesignActionResult>.Success(new ErgonomicsWorkstationDesignActionResult(
                aggregate.ErgonomicsWorkstationDesignId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SubmitForApproval for ErgonomicsWorkstationDesign {ErgonomicsWorkstationDesignId}", request.ErgonomicsWorkstationDesignId);

            return Result<ErgonomicsWorkstationDesignActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.ErgonomicsWorkstationDesign.SubmitForApprovalFailed",
                "Unable to SubmitForApproval the ErgonomicsWorkstationDesign record at this time.",
                Guid.NewGuid()));
        }
    }
}