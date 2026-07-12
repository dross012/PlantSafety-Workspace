// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetInspectionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.Inspection.Queries;
using PlantSafety.Application.Compliance.Procedures.Inspection.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.Inspection.Handlers;

/// <summary>Handles <see cref="GetInspectionQuery"/>.</summary>
public sealed class GetInspectionQueryHandler : IRequestHandler<GetInspectionQuery, Result<InspectionQueryResult>>
{
    private readonly IInspectionRepository _repository;
    private readonly ILogger<GetInspectionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInspectionQueryHandler(IInspectionRepository repository, ILogger<GetInspectionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InspectionQueryResult>> Handle(GetInspectionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetInspectionProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InspectionQueryResult>.Success(new InspectionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Inspection for Tenant {TenantId}", request.TenantId);

            return Result<InspectionQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.Inspection.RetrievalFailed",
                "Unable to retrieve Inspection data at this time.",
                Guid.NewGuid()));
        }
    }
}
