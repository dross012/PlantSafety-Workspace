// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           ActivateComplianceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateComplianceCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Compliance.Interfaces;
using PlantSafety.Domain.Compliance.Compliance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Compliance.Commands;

/// <summary>Handles <see cref="ActivateComplianceCommand"/>.</summary>
public sealed class ActivateComplianceCommandHandler : IRequestHandler<ActivateComplianceCommand, Result<ComplianceStatusResult>>
{
    private readonly IComplianceRepository _repository;
    private readonly ILogger<ActivateComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateComplianceCommandHandler(IComplianceRepository repository, ILogger<ActivateComplianceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceStatusResult>> Handle(ActivateComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ComplianceId.FromGuid(request.ComplianceId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ComplianceStatusResult>.Failure(new ApplicationError(
                    "Compliance.Compliance.NotFound",
                    "The requested compliance record could not be found.",
                    request.ComplianceId,
                    ErrorSeverity.NotFound));
            }

            var activatedBy = UserId.FromGuid(request.ActivatedByUserId);

            aggregate.Activate(
                activatedBy: activatedBy,
                activatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(activatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ComplianceStatusResult>.Success(new ComplianceStatusResult(
                aggregate.ComplianceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate Compliance {ComplianceId}", request.ComplianceId);

            return Result<ComplianceStatusResult>.Failure(new ApplicationError(
                "Compliance.Compliance.ActivateFailed",
                "Unable to activate the compliance record at this time.",
                Guid.NewGuid()));
        }
    }
}
