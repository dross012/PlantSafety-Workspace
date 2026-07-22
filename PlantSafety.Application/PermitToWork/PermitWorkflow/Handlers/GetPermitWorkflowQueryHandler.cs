// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPermitWorkflowQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PermitToWork.PermitWorkflow.Queries;
using PlantSafety.Application.PermitToWork.PermitWorkflow.Repositories;

namespace PlantSafety.Application.PermitToWork.PermitWorkflow.Handlers;

/// <summary>Handles <see cref="GetPermitWorkflowQuery"/>.</summary>
public sealed class GetPermitWorkflowQueryHandler : IRequestHandler<GetPermitWorkflowQuery, Result<PermitWorkflowQueryResult>>
{
    private readonly IPermitWorkflowRepository _repository;
    private readonly ILogger<GetPermitWorkflowQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitWorkflowQueryHandler(IPermitWorkflowRepository repository, ILogger<GetPermitWorkflowQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitWorkflowQueryResult>> Handle(GetPermitWorkflowQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitWorkflowAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitWorkflowQueryResult>.Success(new PermitWorkflowQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitWorkflow for Tenant {TenantId}", request.TenantId);

            return Result<PermitWorkflowQueryResult>.Failure(new ApplicationError(
                "PermitToWork.PermitWorkflow.RetrievalFailed",
                "Unable to retrieve PermitWorkflow data at this time.",
                Guid.NewGuid()));
        }
    }
}