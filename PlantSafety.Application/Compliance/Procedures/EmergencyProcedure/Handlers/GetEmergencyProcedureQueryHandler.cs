// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEmergencyProcedureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Queries;
using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Handlers;

/// <summary>Handles <see cref="GetEmergencyProcedureQuery"/>.</summary>
public sealed class GetEmergencyProcedureQueryHandler : IRequestHandler<GetEmergencyProcedureQuery, Result<EmergencyProcedureQueryResult>>
{
    private readonly IEmergencyProcedureRepository _repository;
    private readonly ILogger<GetEmergencyProcedureQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyProcedureQueryHandler(IEmergencyProcedureRepository repository, ILogger<GetEmergencyProcedureQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyProcedureQueryResult>> Handle(GetEmergencyProcedureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyProcedureQueryResult>.Success(new EmergencyProcedureQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyProcedure for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyProcedureQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.EmergencyProcedure.RetrievalFailed",
                "Unable to retrieve EmergencyProcedure data at this time.",
                Guid.NewGuid()));
        }
    }
}
