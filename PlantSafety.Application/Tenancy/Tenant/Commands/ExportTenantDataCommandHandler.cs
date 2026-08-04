// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ExportTenantDataCommand. See the command's own file header for
//   the export scope note.
// SchemaVersion: 1

using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.MultiRegion.DataResidency;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.Specifications;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using PlantSafety.Application.Tenancy.Tenant.Mapping;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Handles <see cref="ExportTenantDataCommand"/>.</summary>
public sealed class ExportTenantDataCommandHandler(
    ITenantRepository repository,
    ITenantDataExportStore exportStore,
    IDataResidencyEnforcer dataResidencyEnforcer,
    ICurrentDeploymentRegionProvider currentDeploymentRegionProvider,
    ILogger<ExportTenantDataCommandHandler> logger)
    : IRequestHandler<ExportTenantDataCommand, Result<TenantDataExportResultDto>>
{
    /// <inheritdoc/>
    public async Task<Result<TenantDataExportResultDto>> Handle(ExportTenantDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await repository.GetByIdAsync(TenantId.FromGuid(request.TargetTenantId), cancellationToken);
            if (tenant is null)
            {
                return Result<TenantDataExportResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotFound", "The requested tenant was not found.", Guid.NewGuid()));
            }

            if (!TenantSpecification.IsExportable(tenant))
            {
                return Result<TenantDataExportResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NotExportable", $"Tenant {tenant.TenantId} cannot be exported while status is '{tenant.Status.Code}'.", Guid.NewGuid()));
            }

            // Phase 7 §5.3 Data Residency: an export writes the tenant's canonical record to
            // IBlobStorageService, which is physically located in the platform's current
            // deployment region. A tenant whose DataResidencyPolicy forbids that region must have
            // its export rejected here -- silently writing the artifact would itself be the
            // residency violation, not merely a downstream side effect of one.
            try
            {
                dataResidencyEnforcer.EnsureSatisfied(
                    request.TargetTenantId,
                    new CorrelationId(request.CorrelationId),
                    new CausationId(request.CausationId));
            }
            catch (DataResidencyViolationException ex)
            {
                logger.LogWarning(ex,
                    "Tenant {TenantId} data export rejected: residency policy '{Policy}' is not satisfied by the current deployment region '{Region}'.",
                    request.TargetTenantId, ex.RequiredPolicy.Code, ex.CurrentDeploymentRegion.Code);

                return Result<TenantDataExportResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.ExportResidencyViolation",
                    $"Tenant {request.TargetTenantId}'s data-residency policy '{ex.RequiredPolicy.Code}' does not permit exporting from the current deployment region '{ex.CurrentDeploymentRegion.Code}'.",
                    request.CorrelationId,
                    ErrorSeverity.Domain));
            }

            var dto = TenantMapper.ToDto(tenant);
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            var reference = await exportStore.SaveAsync(tenant.TenantId.Value, bytes, cancellationToken);
            var exportedAtUtc = DateTime.UtcNow;
            var validatedRegionCode = currentDeploymentRegionProvider.CurrentRegion.Code;

            // Phase 9 §5.3 Export Lineage Validation: capture the tenant -> region -> destination
            // chain on the record itself, not only on the (rare) rejection path -- a successful
            // export's lineage is exactly as compliance-relevant as a rejected one.
            logger.LogInformation(
                "Tenant {TenantId} data export completed ({Bytes} bytes), region {Region} -> {Reference}",
                tenant.TenantId, bytes.LongLength, validatedRegionCode, reference);

            return Result<TenantDataExportResultDto>.Success(new TenantDataExportResultDto(reference, bytes.LongLength, exportedAtUtc, validatedRegionCode));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to export Tenant {TenantId} data", request.TargetTenantId);

            return Result<TenantDataExportResultDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.ExportFailed", "Unable to export the tenant's data at this time.", Guid.NewGuid()));
        }
    }
}
