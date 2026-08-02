// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           CloseManagementOfChangeCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseManagementOfChangeCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;
using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Handles <see cref="CloseManagementOfChangeCommand"/>.</summary>
public sealed class CloseManagementOfChangeCommandHandler : IRequestHandler<CloseManagementOfChangeCommand, Result<ManagementOfChangeStatusResult>>
{
    private readonly IManagementOfChangeRepository _repository;
    private readonly ILogger<CloseManagementOfChangeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseManagementOfChangeCommandHandler(IManagementOfChangeRepository repository, ILogger<CloseManagementOfChangeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ManagementOfChangeStatusResult>> Handle(CloseManagementOfChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ManagementOfChangeId.FromGuid(request.ManagementOfChangeId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ManagementOfChangeStatusResult>.Failure(new ApplicationError(
                    "Compliance.ManagementOfChange.NotFound",
                    "The requested change record could not be found.",
                    request.ManagementOfChangeId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                closedBy: performedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ManagementOfChangeStatusResult>.Success(new ManagementOfChangeStatusResult(
                aggregate.ManagementOfChangeId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close ManagementOfChange {ManagementOfChangeId}", request.ManagementOfChangeId);

            return Result<ManagementOfChangeStatusResult>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.CloseFailed",
                "Unable to close the change record at this time.",
                Guid.NewGuid()));
        }
    }
}
