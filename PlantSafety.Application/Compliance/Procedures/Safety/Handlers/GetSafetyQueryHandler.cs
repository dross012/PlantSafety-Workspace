// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.Safety.Queries;
using PlantSafety.Application.Compliance.Procedures.Safety.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.Safety.Handlers;

/// <summary>Handles <see cref="GetSafetyQuery"/>.</summary>
public sealed class GetSafetyQueryHandler : IRequestHandler<GetSafetyQuery, Result<SafetyQueryResult>>
{
    private readonly ISafetyRepository _repository;
    private readonly ILogger<GetSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyQueryHandler(ISafetyRepository repository, ILogger<GetSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyQueryResult>> Handle(GetSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSafetyProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyQueryResult>.Success(new SafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Safety for Tenant {TenantId}", request.TenantId);

            return Result<SafetyQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.Safety.RetrievalFailed",
                "Unable to retrieve Safety data at this time.",
                Guid.NewGuid()));
        }
    }
}
