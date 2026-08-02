// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           GetContractorVisitorByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetContractorVisitorByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it belongs
//                  to a different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorVisitor.Models;
using PlantSafety.Application.ContractorVisitor.Queries;
using PlantSafety.Application.ContractorVisitor.Repositories;

namespace PlantSafety.Application.ContractorVisitor.Handlers;

/// <summary>Handles <see cref="GetContractorVisitorByIdQuery"/>.</summary>
public sealed class GetContractorVisitorByIdQueryHandler : IRequestHandler<GetContractorVisitorByIdQuery, Result<ContractorVisitorItemDto>>
{
    private readonly IContractorVisitorRepository _repository;
    private readonly ILogger<GetContractorVisitorByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetContractorVisitorByIdQueryHandler(IContractorVisitorRepository repository, ILogger<GetContractorVisitorByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContractorVisitorItemDto>> Handle(GetContractorVisitorByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ContractorVisitorId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("ContractorVisitor {ContractorVisitorId} not found for Tenant {TenantId}", request.ContractorVisitorId, request.TenantId);

                return Result<ContractorVisitorItemDto>.Failure(new ApplicationError(
                    "ContractorVisitor.NotFound",
                    "The requested contractor/visitor record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ContractorVisitorItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ContractorVisitor {ContractorVisitorId}", request.ContractorVisitorId);

            return Result<ContractorVisitorItemDto>.Failure(new ApplicationError(
                "ContractorVisitor.RetrievalFailed",
                "Unable to retrieve contractor/visitor data at this time.",
                Guid.NewGuid()));
        }
    }
}
