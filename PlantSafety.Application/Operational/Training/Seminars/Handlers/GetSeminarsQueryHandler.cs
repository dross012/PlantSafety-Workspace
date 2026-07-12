// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSeminarsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Training.Seminars.Queries;
using PlantSafety.Application.Operational.Training.Seminars.Repositories;

namespace PlantSafety.Application.Operational.Training.Seminars.Handlers;

/// <summary>Handles <see cref="GetSeminarsQuery"/>.</summary>
public sealed class GetSeminarsQueryHandler : IRequestHandler<GetSeminarsQuery, Result<SeminarsQueryResult>>
{
    private readonly ISeminarsRepository _repository;
    private readonly ILogger<GetSeminarsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSeminarsQueryHandler(ISeminarsRepository repository, ILogger<GetSeminarsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SeminarsQueryResult>> Handle(GetSeminarsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSeminarsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SeminarsQueryResult>.Success(new SeminarsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Seminars for Tenant {TenantId}", request.TenantId);

            return Result<SeminarsQueryResult>.Failure(new ApplicationError(
                "Operational.Training.Seminars.RetrievalFailed",
                "Unable to retrieve seminar data at this time.",
                Guid.NewGuid()));
        }
    }
}
