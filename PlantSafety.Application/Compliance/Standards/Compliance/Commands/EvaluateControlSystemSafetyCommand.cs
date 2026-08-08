// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.ControlSystemSafety
//  Layer:          Application
//  File:           EvaluateControlSystemSafetyCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate ISO 13849-1 / IEC 62061 safety-related-control-system
//                  compliance for a tenant/site's real SafetyValvesRelays and
//                  PresenceSensingDevices components. This codebase does not yet track a formal
//                  Performance Level/Category rating on either real aggregate (see the Domain
//                  engine's own ControlSystemSafetyInput.cs remarks), so the required/achieved
//                  PL and Category are caller-supplied per component, keyed by the component's
//                  own title (ComponentTitle/DeviceTitle) so the handler can join the
//                  classification against the real reliability/inspection data it loads. Any real
//                  component the caller did not supply a classification for is still evaluated
//                  (defaulted to the lowest required Performance Level, PLa, with no achieved
//                  rating -- surfacing as a "no formal PL/Category rating" warning from the
//                  engine) rather than being silently dropped. Carries the full command envelope
//                  per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;
using PlantSafety.Domain.Compliance.Standards.ControlSystemSafety.Enums;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>
/// Caller-supplied ISO 13849-1 Performance Level/Category classification for a single real
/// SafetyValvesRelays/PresenceSensingDevices component, keyed by that component's own title.
/// </summary>
/// <param name="ComponentIdentifier">The real component's ComponentTitle (SafetyValvesRelays) or DeviceTitle (PresenceSensingDevices), matched case-insensitively.</param>
/// <param name="RequiredPerformanceLevel">The Performance Level required for this safety function, per a documented risk assessment.</param>
/// <param name="AchievedPerformanceLevel">The Performance Level actually achieved by this component's design, if formally rated.</param>
/// <param name="Category">The ISO 13849-1 architectural Category of this component, if formally rated.</param>
public sealed record ControlSystemComponentClassificationInput(
    string ComponentIdentifier,
    PerformanceLevel RequiredPerformanceLevel,
    PerformanceLevel? AchievedPerformanceLevel,
    ControlSystemCategory? Category);

/// <summary>Evaluates ISO 13849-1 / IEC 62061 safety-related-control-system compliance for a tenant/site's real SafetyValvesRelays and PresenceSensingDevices components.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="SiteId">Site whose control-system safety posture is being evaluated.</param>
/// <param name="Classifications">Caller-supplied per-component PL/Category classifications, matched against the real components loaded for this tenant/site.</param>
public sealed record EvaluateControlSystemSafetyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    Guid SiteId,
    IReadOnlyList<ControlSystemComponentClassificationInput> Classifications) : ICommand<Result<StandardsComplianceReportDto>>;
