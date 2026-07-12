// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetClassesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Training.Classes.Queries;
using PlantSafety.Application.Operational.Training.Classes.Repositories;

namespace PlantSafety.Application.Operational.Training.Classes.Handlers;

/// <summary>Handles <see cref="GetClassesQuery"/>.</summary>
public sealed class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, Result<ClassesQueryResult>>
{
    private readonly IClassesRepository _repository;
    private readonly ILogger<GetClassesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetClassesQueryHandler(IClassesRepository repository, ILogger<GetClassesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ClassesQueryResult>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetClassesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ClassesQueryResult>.Success(new ClassesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Classes for Tenant {TenantId}", request.TenantId);

            return Result<ClassesQueryResult>.Failure(new ApplicationError(
                "Operational.Training.Classes.RetrievalFailed",
                "Unable to retrieve class data at this time.",
                Guid.NewGuid()));
        }
    }
}
