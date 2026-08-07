// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Application
//  File:           SegregationEvaluationResultDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Application-layer DTO shape mirroring
//                  PlantSafety.Domain.Compliance.PHMSA.Segregation.Services.
//                  SegregationEvaluationResult, so the Segregation module's Queries never leak
//                  Domain-layer value objects (HazardClass, SegregationViolation) across the
//                  Application/Api boundary. There is no persisted aggregate for Segregation --
//                  it is a pure calculation over caller-supplied load items.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Segregation.Models;

/// <summary>A single hazard-class item within a proposed load, as supplied by the caller.</summary>
/// <param name="MaterialId">Identity of the classified material (Classification module).</param>
/// <param name="HazardClass">DOT hazard class/division discriminator (e.g. "3", "1.1", "8").</param>
public sealed record SegregationLoadItemDto(Guid MaterialId, string HazardClass);

/// <summary>A single segregation incompatibility found between two hazard classes in a load.</summary>
/// <param name="ClassA">First hazard class in the incompatible pair.</param>
/// <param name="ClassB">Second hazard class in the incompatible pair.</param>
/// <param name="Reason">Human-readable reason for the incompatibility.</param>
/// <param name="Severity">Severity of the violation ("Advisory" or "Prohibited").</param>
public sealed record SegregationViolationDto(string ClassA, string ClassB, string Reason, string Severity);

/// <summary>Result of evaluating a proposed load for segregation incompatibilities.</summary>
/// <param name="IsCompliant">Whether the load has zero segregation violations.</param>
/// <param name="Violations">Segregation incompatibilities found across the load's item pairs.</param>
/// <param name="RecommendedLoadPlan">Human-readable load-plan recommendations addressing each violation.</param>
public sealed record SegregationEvaluationResultDto(
    bool IsCompliant,
    IReadOnlyList<SegregationViolationDto> Violations,
    IReadOnlyList<string> RecommendedLoadPlan);
