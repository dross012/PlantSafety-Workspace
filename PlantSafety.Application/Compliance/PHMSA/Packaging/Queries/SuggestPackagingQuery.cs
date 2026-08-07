// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           SuggestPackagingQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to determine DOT-authorized packaging for a given hazard class/packing
//                  group/quantity combination. Pure query -- invokes
//                  PackagingEngine.DeterminePackaging directly, no persistence.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Queries;

/// <summary>Determines DOT-authorized packaging for a hazard class/packing group/quantity combination.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazardClass">Primary DOT hazard class, 1-9.</param>
/// <param name="HazardClassDivision">Optional division within the class (e.g. "1" in Class 1.1).</param>
/// <param name="PackingGroup">DOT packing group (I/II/III/NotApplicable).</param>
/// <param name="Quantity">Quantity of material to be packaged.</param>
/// <param name="QuantityUnit">Unit of measure for <paramref name="Quantity"/>.</param>
/// <param name="ContainerTypeRequested">Requested container type/form factor.</param>
public sealed record SuggestPackagingQuery(
    Guid TenantId,
    int HazardClass,
    string? HazardClassDivision,
    string PackingGroup,
    decimal Quantity,
    string QuantityUnit,
    string ContainerTypeRequested) : IRequest<Result<SuggestPackagingResult>>;

/// <summary>Result of <see cref="SuggestPackagingQuery"/>.</summary>
/// <param name="AuthorizedPackaging">DOT packaging specification codes authorized for the given combination.</param>
/// <param name="ClosureInstructions">Closure instructions applicable to the authorized packaging.</param>
/// <param name="Messages">Explanatory messages from the packaging engine.</param>
public sealed record SuggestPackagingResult(
    IReadOnlyList<string> AuthorizedPackaging,
    string ClosureInstructions,
    IReadOnlyList<string> Messages);
