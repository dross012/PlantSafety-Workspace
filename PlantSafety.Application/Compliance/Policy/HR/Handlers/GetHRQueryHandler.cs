// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHRQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.HR.Queries;
using PlantSafety.Application.Compliance.Policy.HR.Repositories;

namespace PlantSafety.Application.Compliance.Policy.HR.Handlers;

/// <summary>Handles <see cref="GetHRQuery"/>.</summary>
public sealed class GetHRQueryHandler : IRequestHandler<GetHRQuery, Result<HRQueryResult>>
{
    private readonly IHRRepository _repository;
    private readonly ILogger<GetHRQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHRQueryHandler(IHRRepository repository, ILogger<GetHRQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HRQueryResult>> Handle(GetHRQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHRPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HRQueryResult>.Success(new HRQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HR for Tenant {TenantId}", request.TenantId);

            return Result<HRQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.HR.RetrievalFailed",
                "Unable to retrieve HR data at this time.",
                Guid.NewGuid()));
        }
    }
}
