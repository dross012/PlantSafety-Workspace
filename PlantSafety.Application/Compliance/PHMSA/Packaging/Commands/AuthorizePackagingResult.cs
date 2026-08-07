// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           AuthorizePackagingResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Result payload for AuthorizePackagingCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Result of a successful <see cref="AuthorizePackagingCommand"/> execution.</summary>
/// <param name="PackagingSpecificationId">Identity of the newly authorized PackagingSpecification aggregate.</param>
/// <param name="DOTSpec">DOT packaging specification code that was authorized.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record AuthorizePackagingResult(
    Guid PackagingSpecificationId,
    string DOTSpec,
    string Status);
