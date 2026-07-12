// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetWorkInstructionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Queries;
using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.WorkInstructions.Handlers;

/// <summary>Handles <see cref="GetWorkInstructionsQuery"/>.</summary>
public sealed class GetWorkInstructionsQueryHandler : IRequestHandler<GetWorkInstructionsQuery, Result<WorkInstructionsQueryResult>>
{
    private readonly IWorkInstructionsRepository _repository;
    private readonly ILogger<GetWorkInstructionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkInstructionsQueryHandler(IWorkInstructionsRepository repository, ILogger<GetWorkInstructionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkInstructionsQueryResult>> Handle(GetWorkInstructionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkInstructionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkInstructionsQueryResult>.Success(new WorkInstructionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkInstructions for Tenant {TenantId}", request.TenantId);

            return Result<WorkInstructionsQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.WorkInstructions.RetrievalFailed",
                "Unable to retrieve WorkInstructions data at this time.",
                Guid.NewGuid()));
        }
    }
}
