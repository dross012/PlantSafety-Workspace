// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           EmergencyResponseInfo.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Value object carrying the emergency response information required to
//                  accompany a hazmat shipment per 49 CFR 172 Subpart G.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;

/// <summary>Emergency response information required to accompany a hazmat shipment.</summary>
public sealed record EmergencyResponseInfo(
    string ErgGuideNumber,
    string EmergencyContactName,
    string EmergencyContactPhone,
    string ResponseGuidanceSummary);
