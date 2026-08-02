// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           CreateContractorVisitorCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateContractorVisitorCommand. Pure orchestration: constructs the
//                  ContractorVisitor aggregate via its domain factory method and persists it
//                  through the domain repository interface. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContractorVisitor.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorVisitor.Commands;

/// <summary>Handles <see cref="CreateContractorVisitorCommand"/>.</summary>
public sealed class CreateContractorVisitorCommandHandler : IRequestHandler<CreateContractorVisitorCommand, Result<CreateContractorVisitorResult>>
{
    private readonly IContractorVisitorRepository _repository;
    private readonly ILogger<CreateContractorVisitorCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateContractorVisitorCommandHandler(IContractorVisitorRepository repository, ILogger<CreateContractorVisitorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateContractorVisitorResult>> Handle(CreateContractorVisitorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var hostId = UserId.FromGuid(request.HostUserId);

            var aggregate = PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                fullName: request.FullName,
                companyName: request.CompanyName,
                visitorType: request.VisitorType,
                purposeOfVisit: request.PurposeOfVisit,
                hostUserId: hostId,
                scheduledArrivalUtc: request.ScheduledArrivalUtc.HasValue ? UtcTimestamp.From(request.ScheduledArrivalUtc.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateContractorVisitorResult>.Success(new CreateContractorVisitorResult(
                aggregate.ContractorVisitorId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ContractorVisitor for Tenant {TenantId}", request.TenantId);

            return Result<CreateContractorVisitorResult>.Failure(new ApplicationError(
                "ContractorVisitor.CreateFailed",
                "Unable to register the contractor/visitor record at this time.",
                Guid.NewGuid()));
        }
    }
}
