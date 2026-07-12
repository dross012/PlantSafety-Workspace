// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetErgonomicsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Ergonomics.Queries;
using PlantSafety.Application.Operational.Ergonomics.Repositories;

namespace PlantSafety.Application.Operational.Ergonomics.Handlers;

/// <summary>Handles <see cref="GetErgonomicsQuery"/>.</summary>
public sealed class GetErgonomicsQueryHandler : IRequestHandler<GetErgonomicsQuery, Result<ErgonomicsQueryResult>>
{
    private readonly IErgonomicsRepository _repository;
    private readonly ILogger<GetErgonomicsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetErgonomicsQueryHandler(IErgonomicsRepository repository, ILogger<GetErgonomicsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsQueryResult>> Handle(GetErgonomicsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetErgonomicsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ErgonomicsQueryResult>.Success(new ErgonomicsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Ergonomics for Tenant {TenantId}", request.TenantId);

            return Result<ErgonomicsQueryResult>.Failure(new ApplicationError(
                "Operational.Ergonomics.RetrievalFailed",
                "Unable to retrieve Ergonomics data at this time.",
                Guid.NewGuid()));
        }
    }
}
