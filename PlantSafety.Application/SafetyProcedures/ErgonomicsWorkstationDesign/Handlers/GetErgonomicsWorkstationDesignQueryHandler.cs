// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetErgonomicsWorkstationDesignQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Queries;
using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Repositories;

namespace PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Handlers;

/// <summary>Handles <see cref="GetErgonomicsWorkstationDesignQuery"/>.</summary>
public sealed class GetErgonomicsWorkstationDesignQueryHandler : IRequestHandler<GetErgonomicsWorkstationDesignQuery, Result<ErgonomicsWorkstationDesignQueryResult>>
{
    private readonly IErgonomicsWorkstationDesignRepository _repository;
    private readonly ILogger<GetErgonomicsWorkstationDesignQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetErgonomicsWorkstationDesignQueryHandler(IErgonomicsWorkstationDesignRepository repository, ILogger<GetErgonomicsWorkstationDesignQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsWorkstationDesignQueryResult>> Handle(GetErgonomicsWorkstationDesignQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetErgonomicsWorkstationDesignItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ErgonomicsWorkstationDesignQueryResult>.Success(new ErgonomicsWorkstationDesignQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ErgonomicsWorkstationDesign records for Tenant {TenantId}", request.TenantId);

            return Result<ErgonomicsWorkstationDesignQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.ErgonomicsWorkstationDesign.RetrievalFailed",
                "Unable to retrieve ErgonomicsWorkstationDesign records at this time.",
                Guid.NewGuid()));
        }
    }
}