// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetMOCWorkflowsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Queries;
using PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Repositories;

namespace PlantSafety.Application.Compliance.ManagementOfChange.MOCWorkflows.Handlers;

/// <summary>Handles <see cref="GetMOCWorkflowsQuery"/>.</summary>
public sealed class GetMOCWorkflowsQueryHandler : IRequestHandler<GetMOCWorkflowsQuery, Result<MOCWorkflowsQueryResult>>
{
    private readonly IMOCWorkflowsRepository _repository;
    private readonly ILogger<GetMOCWorkflowsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMOCWorkflowsQueryHandler(IMOCWorkflowsRepository repository, ILogger<GetMOCWorkflowsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MOCWorkflowsQueryResult>> Handle(GetMOCWorkflowsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMOCWorkflowsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MOCWorkflowsQueryResult>.Success(new MOCWorkflowsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MOCWorkflows for Tenant {TenantId}", request.TenantId);

            return Result<MOCWorkflowsQueryResult>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.MOCWorkflows.RetrievalFailed",
                "Unable to retrieve Management of Change workflow data at this time.",
                Guid.NewGuid()));
        }
    }
}
