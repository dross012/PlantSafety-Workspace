// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for QualifyDriverCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="QualifyDriverCommand"/>.</summary>
public sealed class QualifyDriverCommandHandler : IRequestHandler<QualifyDriverCommand, Result<DriverQualificationActionResult>>
{
    private readonly IDriverQualificationRepository _repository;
    private readonly ILogger<QualifyDriverCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public QualifyDriverCommandHandler(IDriverQualificationRepository repository, ILogger<QualifyDriverCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DriverQualificationActionResult>> Handle(QualifyDriverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<DriverQualificationType>(request.QualificationType, ignoreCase: true, out var qualificationType))
            {
                return Result<DriverQualificationActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.DriverQualifications.InvalidQualificationType",
                    $"'{request.QualificationType}' is not a recognised driver qualification type.",
                    Guid.NewGuid()));
            }

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                driverUserId: request.DriverUserId,
                qualificationType: qualificationType,
                licenseNumber: request.LicenseNumber,
                issuedAtUtc: UtcTimestamp.From(request.IssuedAtUtc),
                expiresAtUtc: UtcTimestamp.From(request.ExpiresAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<DriverQualificationActionResult>.Success(new DriverQualificationActionResult(
                aggregate.DriverQualificationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to qualify driver for Tenant {TenantId}", request.TenantId);

            return Result<DriverQualificationActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.DriverQualifications.QualifyFailed",
                "Unable to qualify the driver at this time.",
                Guid.NewGuid()));
        }
    }
}
