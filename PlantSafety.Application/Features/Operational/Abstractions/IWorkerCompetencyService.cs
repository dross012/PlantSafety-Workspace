// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the Operational module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Abstractions;

/// <summary>Evaluates worker competency and training compliance.</summary>
public interface IWorkerCompetencyService
{
    /// <summary>Returns <c>true</c> when the worker holds all required competencies for the given task.</summary>
    Task<bool> IsCompetentForTaskAsync(
        Guid tenantId,
        Guid workerId,
        string taskCode,
        CancellationToken cancellationToken);

    /// <summary>Returns the training compliance percentage for the site (0.0–100.0).</summary>
    Task<decimal> GetSiteComplianceRateAsync(
        Guid tenantId,
        Guid siteId,
        CancellationToken cancellationToken);
}

/// <summary>Manages PPE stock levels and issuance eligibility.</summary>
public interface IPpeInventoryService
{
    /// <summary>Returns <c>true</c> when sufficient PPE stock is available for the requested quantity.</summary>
    Task<bool> IsAvailableAsync(
        Guid tenantId,
        string ppeCode,
        int quantity,
        CancellationToken cancellationToken);
}
