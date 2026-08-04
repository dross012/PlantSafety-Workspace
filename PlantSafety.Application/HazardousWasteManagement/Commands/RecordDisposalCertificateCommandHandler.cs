// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordDisposalCertificateCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="RecordDisposalCertificateCommand"/>.</summary>
public sealed class RecordDisposalCertificateCommandHandler : IRequestHandler<RecordDisposalCertificateCommand, Result<DisposalCertificateActionResult>>
{
    private readonly IDisposalCertificateRepository _repository;
    private readonly ILogger<RecordDisposalCertificateCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordDisposalCertificateCommandHandler(IDisposalCertificateRepository repository, ILogger<RecordDisposalCertificateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DisposalCertificateActionResult>> Handle(RecordDisposalCertificateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var now = UtcTimestamp.Now();

            if (!Enum.TryParse<DisposalMethod>(request.DisposalMethod, ignoreCase: true, out var disposalMethod))
                disposalMethod = DisposalMethod.Landfill;

            var aggregate = DisposalCertificate.Create(
                performedBy, now,
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId), PerformedByActor.User(performedBy),
                request.TenantId, request.SiteId, request.ManifestId, request.TsdfFacilityId,
                disposalMethod, new UtcTimestamp(request.DisposalDateUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DisposalCertificateActionResult>.Success(new DisposalCertificateActionResult(
                aggregate.DisposalCertificateId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordDisposalCertificate for Tenant {TenantId}", request.TenantId);

            return Result<DisposalCertificateActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.DisposalCertificate.RecordFailed",
                "Unable to record the disposal certificate at this time.",
                Guid.NewGuid()));
        }
    }
}
