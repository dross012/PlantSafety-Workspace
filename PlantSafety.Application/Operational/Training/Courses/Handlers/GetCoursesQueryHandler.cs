// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCoursesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Training.Courses.Queries;
using PlantSafety.Application.Operational.Training.Courses.Repositories;

namespace PlantSafety.Application.Operational.Training.Courses.Handlers;

/// <summary>Handles <see cref="GetCoursesQuery"/>.</summary>
public sealed class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, Result<CoursesQueryResult>>
{
    private readonly ICoursesRepository _repository;
    private readonly ILogger<GetCoursesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCoursesQueryHandler(ICoursesRepository repository, ILogger<GetCoursesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CoursesQueryResult>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCoursesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CoursesQueryResult>.Success(new CoursesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Courses for Tenant {TenantId}", request.TenantId);

            return Result<CoursesQueryResult>.Failure(new ApplicationError(
                "Operational.Training.Courses.RetrievalFailed",
                "Unable to retrieve course data at this time.",
                Guid.NewGuid()));
        }
    }
}
