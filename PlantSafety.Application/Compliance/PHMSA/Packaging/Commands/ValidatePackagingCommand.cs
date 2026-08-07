// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ValidatePackagingCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to validate whether a proposed DOT packaging spec is authorized for a
//                  given hazard class/packing group combination. Deliberately modeled as a
//                  Command (per the module's build spec) even though it is read-only/
//                  side-effect-free -- it invokes PackagingEngine.ValidatePackaging and returns
//                  the result directly with no persistence. Carries the full command envelope for
//                  consistency with the module's other commands.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Commands;

/// <summary>Validates whether a proposed DOT packaging spec is authorized for a hazard class/packing group.</summary>
/// <param name="ProposedDOTSpec">DOT packaging specification code being validated, e.g. "1A1".</param>
/// <param name="HazardClass">Primary DOT hazard class, 1-9.</param>
/// <param name="HazardClassDivision">Optional division within the class (e.g. "1" in Class 1.1).</param>
/// <param name="PackingGroup">DOT packing group (I/II/III/NotApplicable).</param>
/// <param name="RequestedByUserId">Identity of the user requesting the validation.</param>
public sealed record ValidatePackagingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    string ProposedDOTSpec,
    int HazardClass,
    string? HazardClassDivision,
    string PackingGroup,
    Guid RequestedByUserId) : ICommand<Result<ValidatePackagingResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ValidatePackagingResult>>.CausationId => CausationId;
}

/// <summary>Result of <see cref="ValidatePackagingCommand"/>.</summary>
/// <param name="IsAuthorized">Whether the proposed spec is DOT-authorized for the given hazard class/packing group.</param>
/// <param name="Message">Explanatory message from the packaging engine.</param>
public sealed record ValidatePackagingResult(bool IsAuthorized, string Message);
