// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPEAssignmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.PPEAssignment.Queries;
using PlantSafety.Application.PPE.PPEAssignment.Repositories;

namespace PlantSafety.Application.PPE.PPEAssignment.Handlers;

/// <summary>Handles <see cref="GetPPEAssignmentQuery"/>.</summary>
public sealed class GetPPEAssignmentQueryHandler : IRequestHandler<GetPPEAssignmentQuery, Result<PPEAssignmentQueryResult>>
{
    private readonly IPPEAssignmentRepository _repository;
    private readonly ILogger<GetPPEAssignmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEAssignmentQueryHandler(IPPEAssignmentRepository repository, ILogger<GetPPEAssignmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEAssignmentQueryResult>> Handle(GetPPEAssignmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEAssignmentAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEAssignmentQueryResult>.Success(new PPEAssignmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPEAssignment for Tenant {TenantId}", request.TenantId);

            return Result<PPEAssignmentQueryResult>.Failure(new ApplicationError(
                "PPE.PPEAssignment.RetrievalFailed",
                "Unable to retrieve PPEAssignment data at this time.",
                Guid.NewGuid()));
        }
    }
}