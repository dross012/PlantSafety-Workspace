// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPressureTestingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.PressureTesting.Queries;
using PlantSafety.Application.Compliance.Permits.PressureTesting.Repositories;

namespace PlantSafety.Application.Compliance.Permits.PressureTesting.Handlers;

/// <summary>Handles <see cref="GetPressureTestingQuery"/>.</summary>
public sealed class GetPressureTestingQueryHandler : IRequestHandler<GetPressureTestingQuery, Result<PressureTestingQueryResult>>
{
    private readonly IPressureTestingRepository _repository;
    private readonly ILogger<GetPressureTestingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPressureTestingQueryHandler(IPressureTestingRepository repository, ILogger<GetPressureTestingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PressureTestingQueryResult>> Handle(GetPressureTestingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPressureTestingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PressureTestingQueryResult>.Success(new PressureTestingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PressureTesting for Tenant {TenantId}", request.TenantId);

            return Result<PressureTestingQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.PressureTesting.RetrievalFailed",
                "Unable to retrieve pressure testing permit data at this time.",
                Guid.NewGuid()));
        }
    }
}
