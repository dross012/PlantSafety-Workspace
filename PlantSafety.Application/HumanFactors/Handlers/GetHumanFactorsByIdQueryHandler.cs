// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           GetHumanFactorsByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetHumanFactorsByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.Models;
using PlantSafety.Application.HumanFactors.Queries;
using PlantSafety.Application.HumanFactors.Repositories;

namespace PlantSafety.Application.HumanFactors.Handlers;

/// <summary>Handles <see cref="GetHumanFactorsByIdQuery"/>.</summary>
public sealed class GetHumanFactorsByIdQueryHandler : IRequestHandler<GetHumanFactorsByIdQuery, Result<HumanFactorsItemDto>>
{
    private readonly IHumanFactorsRepository _repository;
    private readonly ILogger<GetHumanFactorsByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHumanFactorsByIdQueryHandler(IHumanFactorsRepository repository, ILogger<GetHumanFactorsByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HumanFactorsItemDto>> Handle(GetHumanFactorsByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.HumanFactorsId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("HumanFactors {HumanFactorsId} not found for Tenant {TenantId}", request.HumanFactorsId, request.TenantId);

                return Result<HumanFactorsItemDto>.Failure(new ApplicationError(
                    "HumanFactors.NotFound",
                    "The requested human-factors record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<HumanFactorsItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HumanFactors {HumanFactorsId}", request.HumanFactorsId);

            return Result<HumanFactorsItemDto>.Failure(new ApplicationError(
                "HumanFactors.RetrievalFailed",
                "Unable to retrieve human-factors data at this time.",
                Guid.NewGuid()));
        }
    }
}
