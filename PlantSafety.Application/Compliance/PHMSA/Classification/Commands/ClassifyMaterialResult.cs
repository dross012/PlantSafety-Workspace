// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ClassifyMaterialResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for ClassifyMaterialCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Classification.Commands;

/// <summary>Result of a successful <see cref="ClassifyMaterialCommand"/> execution.</summary>
/// <param name="HazmatMaterialId">Identity of the newly classified HazmatMaterial aggregate.</param>
/// <param name="UNNumber">DOT/UN identification number assigned by the classification engine.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record ClassifyMaterialResult(
    Guid HazmatMaterialId,
    string UNNumber,
    string Status);
