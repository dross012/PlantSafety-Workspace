// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetDepartmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Department.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Department.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Department.Handlers;

/// <summary>Handles <see cref="GetDepartmentQuery"/>.</summary>
public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, Result<DepartmentQueryResult>>
{
    private readonly IDepartmentRepository _repository;
    private readonly ILogger<GetDepartmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDepartmentQueryHandler(IDepartmentRepository repository, ILogger<GetDepartmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DepartmentQueryResult>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDepartmentScoresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<DepartmentQueryResult>.Success(new DepartmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Department for Tenant {TenantId}", request.TenantId);

            return Result<DepartmentQueryResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.Department.RetrievalFailed",
                "Unable to retrieve Department data at this time.",
                Guid.NewGuid()));
        }
    }
}
