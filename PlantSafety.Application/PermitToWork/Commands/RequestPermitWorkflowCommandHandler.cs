// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RequestPermitWorkflowCommand. Pure orchestration: constructs the
// PermitWorkflow aggregate via its domain factory method and persists it through the domain
// repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Handles <see cref="RequestPermitWorkflowCommand"/>.</summary>
public sealed class RequestPermitWorkflowCommandHandler : IRequestHandler<RequestPermitWorkflowCommand, Result<PermitWorkflowActionResult>>
{
    private readonly IPermitWorkflowRepository _repository;
    private readonly ILogger<RequestPermitWorkflowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RequestPermitWorkflowCommandHandler(IPermitWorkflowRepository repository, ILogger<RequestPermitWorkflowCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitWorkflowActionResult>> Handle(RequestPermitWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                permitNumber: request.PermitNumber,
                permitTypeCode: request.PermitTypeCode,
                requesterUserId: requesterId,
                issuerUserId: null,
                workDescription: request.WorkDescription);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PermitWorkflowActionResult>.Success(new PermitWorkflowActionResult(
                aggregate.PermitWorkflowId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request PermitWorkflow for Tenant {TenantId}", request.TenantId);

            return Result<PermitWorkflowActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitWorkflow.RequestFailed",
                "Unable to submit the permit request at this time.",
                Guid.NewGuid()));
        }
    }
}
