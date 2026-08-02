// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           GetProceduresByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetProceduresByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.Models;
using PlantSafety.Application.Compliance.Procedures.Queries;
using PlantSafety.Application.Compliance.Procedures.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.Handlers;

/// <summary>Handles <see cref="GetProceduresByIdQuery"/>.</summary>
public sealed class GetProceduresByIdQueryHandler : IRequestHandler<GetProceduresByIdQuery, Result<ProceduresItemDto>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<GetProceduresByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProceduresByIdQueryHandler(IProceduresRepository repository, ILogger<GetProceduresByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProceduresItemDto>> Handle(GetProceduresByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ProceduresId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Procedures {ProceduresId} not found for Tenant {TenantId}", request.ProceduresId, request.TenantId);

                return Result<ProceduresItemDto>.Failure(new ApplicationError(
                    "Compliance.Procedures.NotFound",
                    "The requested procedure could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ProceduresItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Procedures {ProceduresId}", request.ProceduresId);

            return Result<ProceduresItemDto>.Failure(new ApplicationError(
                "Compliance.Procedures.RetrievalFailed",
                "Unable to retrieve procedure data at this time.",
                Guid.NewGuid()));
        }
    }
}
