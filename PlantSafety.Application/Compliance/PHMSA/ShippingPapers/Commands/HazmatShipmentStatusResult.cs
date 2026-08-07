// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           HazmatShipmentStatusResult.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the HazmatShipment lifecycle-transition commands
//                  (GenerateShippingPaper, MarkShipmentReady, DispatchShipment, CancelShipment).
//                  Mirrors PlantSafety.Application.Compliance.Permits.Commands.PermitsStatusResult.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Commands;

/// <summary>Result payload shared by HazmatShipment lifecycle-transition commands.</summary>
/// <param name="HazmatShipmentId">Identity of the affected HazmatShipment aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record HazmatShipmentStatusResult(Guid HazmatShipmentId, string Status);
