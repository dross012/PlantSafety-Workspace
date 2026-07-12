// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetIndustrialHygieneQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.IndustrialHygiene.Queries;
using PlantSafety.Application.Operational.IndustrialHygiene.Repositories;

namespace PlantSafety.Application.Operational.IndustrialHygiene.Handlers;

/// <summary>Handles <see cref="GetIndustrialHygieneQuery"/>.</summary>
public sealed class GetIndustrialHygieneQueryHandler : IRequestHandler<GetIndustrialHygieneQuery, Result<IndustrialHygieneQueryResult>>
{
    private readonly IIndustrialHygieneRepository _repository;
    private readonly ILogger<GetIndustrialHygieneQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIndustrialHygieneQueryHandler(IIndustrialHygieneRepository repository, ILogger<GetIndustrialHygieneQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IndustrialHygieneQueryResult>> Handle(GetIndustrialHygieneQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIndustrialHygieneAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IndustrialHygieneQueryResult>.Success(new IndustrialHygieneQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IndustrialHygiene for Tenant {TenantId}", request.TenantId);

            return Result<IndustrialHygieneQueryResult>.Failure(new ApplicationError(
                "Operational.IndustrialHygiene.RetrievalFailed",
                "Unable to retrieve IndustrialHygiene data at this time.",
                Guid.NewGuid()));
        }
    }
}
