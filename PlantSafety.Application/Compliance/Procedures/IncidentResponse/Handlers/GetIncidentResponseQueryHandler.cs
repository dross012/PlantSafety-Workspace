// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetIncidentResponseQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Queries;
using PlantSafety.Application.Compliance.Procedures.IncidentResponse.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.IncidentResponse.Handlers;

/// <summary>Handles <see cref="GetIncidentResponseQuery"/>.</summary>
public sealed class GetIncidentResponseQueryHandler : IRequestHandler<GetIncidentResponseQuery, Result<IncidentResponseQueryResult>>
{
    private readonly IIncidentResponseRepository _repository;
    private readonly ILogger<GetIncidentResponseQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentResponseQueryHandler(IIncidentResponseRepository repository, ILogger<GetIncidentResponseQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentResponseQueryResult>> Handle(GetIncidentResponseQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentResponseProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IncidentResponseQueryResult>.Success(new IncidentResponseQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentResponse for Tenant {TenantId}", request.TenantId);

            return Result<IncidentResponseQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.IncidentResponse.RetrievalFailed",
                "Unable to retrieve IncidentResponse data at this time.",
                Guid.NewGuid()));
        }
    }
}
