// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEmployeesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Employees.Queries;
using PlantSafety.Application.CrossDomain.Employees.Repositories;

namespace PlantSafety.Application.CrossDomain.Employees.Handlers;

/// <summary>Handles <see cref="GetEmployeesQuery"/>.</summary>
public sealed class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<EmployeesQueryResult>>
{
    private readonly IEmployeesRepository _repository;
    private readonly ILogger<GetEmployeesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmployeesQueryHandler(IEmployeesRepository repository, ILogger<GetEmployeesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmployeesQueryResult>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmployeesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmployeesQueryResult>.Success(new EmployeesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Employees for Tenant {TenantId}", request.TenantId);

            return Result<EmployeesQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Employees.RetrievalFailed",
                "Unable to retrieve employees data at this time.",
                Guid.NewGuid()));
        }
    }
}
