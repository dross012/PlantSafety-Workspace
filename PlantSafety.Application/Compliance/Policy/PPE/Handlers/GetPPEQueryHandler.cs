// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPPEQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.PPE.Queries;
using PlantSafety.Application.Compliance.Policy.PPE.Repositories;

namespace PlantSafety.Application.Compliance.Policy.PPE.Handlers;

/// <summary>Handles <see cref="GetPPEQuery"/>.</summary>
public sealed class GetPPEQueryHandler : IRequestHandler<GetPPEQuery, Result<PPEQueryResult>>
{
    private readonly IPPERepository _repository;
    private readonly ILogger<GetPPEQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEQueryHandler(IPPERepository repository, ILogger<GetPPEQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEQueryResult>> Handle(GetPPEQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEQueryResult>.Success(new PPEQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPE for Tenant {TenantId}", request.TenantId);

            return Result<PPEQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.PPE.RetrievalFailed",
                "Unable to retrieve PPE data at this time.",
                Guid.NewGuid()));
        }
    }
}
