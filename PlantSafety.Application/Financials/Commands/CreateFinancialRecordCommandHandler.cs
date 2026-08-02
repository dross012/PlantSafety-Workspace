// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           CreateFinancialRecordCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateFinancialRecordCommand. Pure orchestration: constructs the
//                  FinancialRecord aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Financials.Aggregates;
using PlantSafety.Domain.Financials.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Financials.Commands;

/// <summary>Handles <see cref="CreateFinancialRecordCommand"/>.</summary>
public sealed class CreateFinancialRecordCommandHandler : IRequestHandler<CreateFinancialRecordCommand, Result<CreateFinancialRecordResult>>
{
    private readonly IFinancialRecordRepository _repository;
    private readonly ILogger<CreateFinancialRecordCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateFinancialRecordCommandHandler(IFinancialRecordRepository repository, ILogger<CreateFinancialRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateFinancialRecordResult>> Handle(CreateFinancialRecordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = FinancialRecord.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                category: request.Category,
                amount: request.Amount,
                recordedBy: requesterId,
                recordedAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateFinancialRecordResult>.Success(new CreateFinancialRecordResult(
                aggregate.FinancialsId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create FinancialRecord for Tenant {TenantId}", request.TenantId);

            return Result<CreateFinancialRecordResult>.Failure(new ApplicationError(
                "Financials.FinancialRecord.CreateFailed",
                "Unable to create the financial record at this time.",
                Guid.NewGuid()));
        }
    }
}
