// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPpeQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.Ppe.Queries;
using PlantSafety.Application.EmployeeSafety.Ppe.Repositories;

namespace PlantSafety.Application.EmployeeSafety.Ppe.Handlers;

/// <summary>Handles <see cref="GetPpeQuery"/>.</summary>
public sealed class GetPpeQueryHandler : IRequestHandler<GetPpeQuery, Result<PpeQueryResult>>
{
    private readonly IPpeRepository _repository;
    private readonly ILogger<GetPpeQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPpeQueryHandler(IPpeRepository repository, ILogger<GetPpeQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PpeQueryResult>> Handle(GetPpeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPpeItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PpeQueryResult>.Success(new PpeQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Ppe records for Tenant {TenantId}", request.TenantId);

            return Result<PpeQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.Ppe.RetrievalFailed",
                "Unable to retrieve Ppe records at this time.",
                Guid.NewGuid()));
        }
    }
}