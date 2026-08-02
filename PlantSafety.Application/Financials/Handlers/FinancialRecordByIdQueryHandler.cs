// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           FinancialRecordByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for FinancialRecordByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Handlers.GetRCAByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Financials.Models;
using PlantSafety.Application.Financials.Queries;
using PlantSafety.Application.Financials.Repositories;

namespace PlantSafety.Application.Financials.Handlers;

/// <summary>Handles <see cref="FinancialRecordByIdQuery"/>.</summary>
public sealed class FinancialRecordByIdQueryHandler : IRequestHandler<FinancialRecordByIdQuery, Result<FinancialRecordDetailDto>>
{
    private readonly IFinancialRecordDetailRepository _repository;
    private readonly ILogger<FinancialRecordByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FinancialRecordByIdQueryHandler(IFinancialRecordDetailRepository repository, ILogger<FinancialRecordByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FinancialRecordDetailDto>> Handle(FinancialRecordByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.FinancialRecordId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("FinancialRecord {FinancialRecordId} not found for Tenant {TenantId}", request.FinancialRecordId, request.TenantId);

                return Result<FinancialRecordDetailDto>.Failure(new ApplicationError(
                    "Financials.FinancialRecord.NotFound",
                    "The requested financial record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<FinancialRecordDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FinancialRecord {FinancialRecordId}", request.FinancialRecordId);

            return Result<FinancialRecordDetailDto>.Failure(new ApplicationError(
                "Financials.FinancialRecord.RetrievalFailed",
                "Unable to retrieve financial record data at this time.",
                Guid.NewGuid()));
        }
    }
}
