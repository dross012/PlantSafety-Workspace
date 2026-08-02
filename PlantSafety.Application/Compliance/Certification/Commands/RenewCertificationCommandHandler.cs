// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           RenewCertificationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RenewCertificationCommand. Mirrors VerifyCertificationCommandHandler's
//                  tenant-scoped lookup / NotFound-severity / concurrency-propagation conventions.
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

/// <summary>Handles <see cref="RenewCertificationCommand"/>.</summary>
public sealed class RenewCertificationCommandHandler : IRequestHandler<RenewCertificationCommand, Result<CertificationStatusResult>>
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<RenewCertificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RenewCertificationCommandHandler(ICertificationRepository repository, ILogger<RenewCertificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CertificationStatusResult>> Handle(RenewCertificationCommand request, CancellationToken cancellationToken)
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

            var renewedBy = UserId.FromGuid(request.RenewedByUserId);

            aggregate.Renew(
                renewedBy: renewedBy,
                newExpiresAtUtc: UtcTimestamp.From(request.NewExpiresAtUtc),
                renewedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(renewedBy),
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
            _logger.LogError(ex, "Failed to renew Certification {CertificationId}", request.CertificationId);

            return Result<CertificationStatusResult>.Failure(new ApplicationError(
                "Compliance.Certification.RenewFailed",
                "Unable to renew the certification at this time.",
                Guid.NewGuid()));
        }
    }
}
