// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           CalculatePlacardsCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Pure, side-effect-free command wrapping
//                  PlantSafety.Domain.Compliance.PHMSA.Placarding.Services.PlacardingEngine
//                  directly, with no persistence. Modeled as a command (not a query) because it
//                  is invoked as an action against a caller-supplied load payload rather than a
//                  read against stored state, matching the user's spec wording ("Calculate
//                  placards"). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;

/// <summary>Computes the required DOT transport placard set for a proposed load, with no persistence.</summary>
/// <param name="Materials">The hazard classes present in the load, with quantity and unit.</param>
/// <param name="IsBulkPackaging">Whether the load is carried in bulk packaging.</param>
public sealed record CalculatePlacardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    IReadOnlyList<PlacardingMaterialItem> Materials,
    bool IsBulkPackaging,
    Guid RequestedByUserId) : ICommand<Result<CalculatePlacardsResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CalculatePlacardsResult>>.CausationId => CausationId;
}

/// <summary>Result of <see cref="CalculatePlacardsCommand"/>.</summary>
/// <param name="Success">Whether placarding could be computed for the supplied materials.</param>
/// <param name="RequiredPlacards">The computed set of required placard names.</param>
/// <param name="AnyOrientationArrowsRequired">Whether any material in the load requires orientation arrows.</param>
/// <param name="Messages">Human-readable explanation of the computed result.</param>
public sealed record CalculatePlacardsResult(
    bool Success,
    IReadOnlyList<string> RequiredPlacards,
    bool AnyOrientationArrowsRequired,
    IReadOnlyList<string> Messages);
