// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CloseCertificationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseCertificationCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Certification.Interfaces;
using PlantSafety.Domain.Compliance.Certification.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Certification.Commands;

/// <summary>Handles <see cref="CloseCertificationCommand"/>.</summary>
public sealed class CloseCertificationCommandHandler : IRequestHandler<CloseCertificationCommand, Result<CertificationStatusResult>>
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<CloseCertificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseCertificationCommandHandler(ICertificationRepository repository, ILogger<CloseCertificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CertificationStatusResult>> Handle(CloseCertificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CertificationId.FromGuid(request.CertificationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<CertificationStatusResult>.Failure(new ApplicationError(
                    "Compliance.Certification.NotFound",
                    "The requested certification record could not be found.",
                    request.CertificationId,
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

            return Result<CertificationStatusResult>.Success(new CertificationStatusResult(
                aggregate.CertificationId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Certification {CertificationId}", request.CertificationId);

            return Result<CertificationStatusResult>.Failure(new ApplicationError(
                "Compliance.Certification.CloseFailed",
                "Unable to close the certification at this time.",
                Guid.NewGuid()));
        }
    }
}
