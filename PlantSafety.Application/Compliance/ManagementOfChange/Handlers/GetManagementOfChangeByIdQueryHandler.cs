// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           GetManagementOfChangeByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetManagementOfChangeByIdQuery. Reads from the read-side repository
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
using PlantSafety.Application.Compliance.ManagementOfChange.Models;
using PlantSafety.Application.Compliance.ManagementOfChange.Queries;
using PlantSafety.Application.Compliance.ManagementOfChange.Repositories;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Handlers;

/// <summary>Handles <see cref="GetManagementOfChangeByIdQuery"/>.</summary>
public sealed class GetManagementOfChangeByIdQueryHandler : IRequestHandler<GetManagementOfChangeByIdQuery, Result<ManagementOfChangeItemDto>>
{
    private readonly IManagementOfChangeRepository _repository;
    private readonly ILogger<GetManagementOfChangeByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetManagementOfChangeByIdQueryHandler(IManagementOfChangeRepository repository, ILogger<GetManagementOfChangeByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ManagementOfChangeItemDto>> Handle(GetManagementOfChangeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ManagementOfChangeId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ManagementOfChange {ManagementOfChangeId} not found for Tenant {TenantId}", request.ManagementOfChangeId, request.TenantId);

                return Result<ManagementOfChangeItemDto>.Failure(new ApplicationError(
                    "Compliance.ManagementOfChange.NotFound",
                    "The requested change record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ManagementOfChangeItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ManagementOfChange {ManagementOfChangeId}", request.ManagementOfChangeId);

            return Result<ManagementOfChangeItemDto>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.RetrievalFailed",
                "Unable to retrieve change record data at this time.",
                Guid.NewGuid()));
        }
    }
}
