// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           ArchiveCertificationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveCertificationCommand.
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

/// <summary>Handles <see cref="ArchiveCertificationCommand"/>.</summary>
public sealed class ArchiveCertificationCommandHandler : IRequestHandler<ArchiveCertificationCommand, Result<CertificationStatusResult>>
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<ArchiveCertificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveCertificationCommandHandler(ICertificationRepository repository, ILogger<ArchiveCertificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CertificationStatusResult>> Handle(ArchiveCertificationCommand request, CancellationToken cancellationToken)
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

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to archive Certification {CertificationId}", request.CertificationId);

            return Result<CertificationStatusResult>.Failure(new ApplicationError(
                "Compliance.Certification.ArchiveFailed",
                "Unable to archive the certification at this time.",
                Guid.NewGuid()));
        }
    }
}
