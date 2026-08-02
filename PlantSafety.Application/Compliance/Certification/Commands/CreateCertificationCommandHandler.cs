// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CreateCertificationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateCertificationCommand. Pure orchestration: constructs the
//                  Certification aggregate via its domain factory method and persists it through
//                  the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Certification.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Certification.Commands;

/// <summary>Handles <see cref="CreateCertificationCommand"/>.</summary>
public sealed class CreateCertificationCommandHandler : IRequestHandler<CreateCertificationCommand, Result<CreateCertificationResult>>
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<CreateCertificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateCertificationCommandHandler(ICertificationRepository repository, ILogger<CreateCertificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateCertificationResult>> Handle(CreateCertificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var holderId = UserId.FromGuid(request.HolderWorkerId);

            var aggregate = PlantSafety.Domain.Compliance.Certification.Aggregates.Certification.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                holderWorkerId: holderId,
                certificationName: request.CertificationName,
                certificationBody: request.CertificationBody,
                certificationNumber: request.CertificationNumber,
                issuedAtUtc: request.IssuedAtUtc.HasValue ? UtcTimestamp.From(request.IssuedAtUtc.Value) : null,
                expiresAtUtc: request.ExpiresAtUtc.HasValue ? UtcTimestamp.From(request.ExpiresAtUtc.Value) : null,
                renewalRequired: request.RenewalRequired);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateCertificationResult>.Success(new CreateCertificationResult(
                aggregate.CertificationId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Certification for Tenant {TenantId}", request.TenantId);

            return Result<CreateCertificationResult>.Failure(new ApplicationError(
                "Compliance.Certification.CreateFailed",
                "Unable to submit the certification record at this time.",
                Guid.NewGuid()));
        }
    }
}
