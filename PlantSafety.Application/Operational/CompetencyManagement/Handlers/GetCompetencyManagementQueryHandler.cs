// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCompetencyManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.CompetencyManagement.Queries;
using PlantSafety.Application.Operational.CompetencyManagement.Repositories;

namespace PlantSafety.Application.Operational.CompetencyManagement.Handlers;

/// <summary>Handles <see cref="GetCompetencyManagementQuery"/>.</summary>
public sealed class GetCompetencyManagementQueryHandler : IRequestHandler<GetCompetencyManagementQuery, Result<CompetencyManagementQueryResult>>
{
    private readonly ICompetencyManagementRepository _repository;
    private readonly ILogger<GetCompetencyManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCompetencyManagementQueryHandler(ICompetencyManagementRepository repository, ILogger<GetCompetencyManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CompetencyManagementQueryResult>> Handle(GetCompetencyManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCompetencyManagementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CompetencyManagementQueryResult>.Success(new CompetencyManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CompetencyManagement for Tenant {TenantId}", request.TenantId);

            return Result<CompetencyManagementQueryResult>.Failure(new ApplicationError(
                "Operational.CompetencyManagement.RetrievalFailed",
                "Unable to retrieve CompetencyManagement data at this time.",
                Guid.NewGuid()));
        }
    }
}
