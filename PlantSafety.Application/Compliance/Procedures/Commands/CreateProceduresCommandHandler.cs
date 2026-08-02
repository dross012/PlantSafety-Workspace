// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           CreateProceduresCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateProceduresCommand. Pure orchestration: constructs the
//                  Procedures aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Procedures.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Handles <see cref="CreateProceduresCommand"/>.</summary>
public sealed class CreateProceduresCommandHandler : IRequestHandler<CreateProceduresCommand, Result<CreateProceduresResult>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<CreateProceduresCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateProceduresCommandHandler(IProceduresRepository repository, ILogger<CreateProceduresCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateProceduresResult>> Handle(CreateProceduresCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                procedureTitle: request.ProcedureTitle,
                procedureNumber: request.ProcedureNumber,
                category: request.Category,
                ownerId: requesterId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateProceduresResult>.Success(new CreateProceduresResult(
                aggregate.ProceduresId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Procedures for Tenant {TenantId}", request.TenantId);

            return Result<CreateProceduresResult>.Failure(new ApplicationError(
                "Compliance.Procedures.CreateFailed",
                "Unable to submit the procedure at this time.",
                Guid.NewGuid()));
        }
    }
}
