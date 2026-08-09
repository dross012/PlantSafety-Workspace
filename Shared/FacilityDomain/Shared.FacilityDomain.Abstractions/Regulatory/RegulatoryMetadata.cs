// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Regulatory
//  File:           RegulatoryMetadata.cs
//  Created:        2026-08-08
//  Description:    Phase C.4: static, deterministic metadata describing
//                  each RegulatoryJurisdiction -- a short code and a
//                  typical reporting-window (hours), used by future
//                  compliance workflow phases. Mirrors SeverityMetadata's
//                  precedent.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Regulatory;

/// <summary>Static, deterministic metadata describing a <see cref="RegulatoryJurisdiction"/>.</summary>
public static class RegulatoryMetadata
{
    /// <summary>The short regulator code used in facility program/regulatory profiles (e.g. Shared.FacilityDomain's FacilityRegulatoryProfile).</summary>
    public static string CodeOf(RegulatoryJurisdiction jurisdiction) => jurisdiction switch
    {
        RegulatoryJurisdiction.OSHA => "OSHA",
        RegulatoryJurisdiction.EPA => "EPA",
        RegulatoryJurisdiction.DOT => "DOT",
        RegulatoryJurisdiction.NFPA => "NFPA",
        RegulatoryJurisdiction.State => "STATE",
        RegulatoryJurisdiction.Local => "LOCAL",
        _ => throw new ArgumentOutOfRangeException(nameof(jurisdiction), jurisdiction, "Unknown RegulatoryJurisdiction."),
    };

    /// <summary>The typical maximum reporting window, in hours, for an incident falling under this jurisdiction.</summary>
    public static int TypicalReportingWindowHours(RegulatoryJurisdiction jurisdiction) => jurisdiction switch
    {
        RegulatoryJurisdiction.OSHA => 8,
        RegulatoryJurisdiction.EPA => 24,
        RegulatoryJurisdiction.DOT => 12,
        RegulatoryJurisdiction.NFPA => 24,
        RegulatoryJurisdiction.State => 24,
        RegulatoryJurisdiction.Local => 24,
        _ => throw new ArgumentOutOfRangeException(nameof(jurisdiction), jurisdiction, "Unknown RegulatoryJurisdiction."),
    };
}
