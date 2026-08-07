// ============================================================================
// File: SegregationRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Segregation
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body for the Compliance.PHMSA.Segregation
//              V1 controller. There is no persisted aggregate for Segregation, so neither request
//              carries an ETag.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;

namespace PlantSafety.Api.Compliance.PHMSA.Segregation.Contracts.Requests;

/// <summary>Request body for evaluating a proposed multi-hazmat load for segregation incompatibilities.</summary>
public sealed record EvaluateLoadRequest
{
    /// <summary>The materials/hazard classes present in the proposed load.</summary>
    public required IReadOnlyList<SegregationLoadItemDto> LoadItems { get; init; }
}

/// <summary>Request body for suggesting a recommended load plan for a proposed multi-hazmat load.</summary>
public sealed record SuggestLoadPlanRequest
{
    /// <summary>The materials/hazard classes present in the proposed load.</summary>
    public required IReadOnlyList<SegregationLoadItemDto> LoadItems { get; init; }
}
