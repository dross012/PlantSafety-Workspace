// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           IDriverQualificationRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the DriverQualification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Interfaces;

/// <summary>Domain repository interface for the <see cref="DriverQualification"/> aggregate root.</summary>
public interface IDriverQualificationRepository
{
    /// <summary>Retrieves a DriverQualification aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification?> GetByIdAsync(DriverQualificationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all DriverQualification aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification>> ListAsync(DriverQualificationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new DriverQualification aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing DriverQualification aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification aggregate, CancellationToken cancellationToken = default);
}
