// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Domain
//  File:           SegregationViolation.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object describing a single segregation incompatibility found between two
//                  hazard classes present in a proposed multi-hazmat load.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Segregation.ValueObjects;

/// <summary>A single segregation incompatibility found between two hazard classes in a load.</summary>
public sealed record SegregationViolation(
    HazardClass ClassA,
    HazardClass ClassB,
    string Reason,
    SegregationSeverity Severity);
