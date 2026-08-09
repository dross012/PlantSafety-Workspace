// ============================================================================
//  Project:        AI.RegulatoryInterpretation
//  Module:         Abstractions.Interpretation
//  File:           RegulatoryInterpretationInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.4 (Path E: AI-Driven Safety Automation): the
//                  input RegulatoryInterpretationEngine.Interpret reasons
//                  over. Reuses Shared.FacilityDomain's real HazardType/
//                  SeverityLevel vocabularies -- which jurisdictions apply
//                  is inferred internally by the engine via Phase C.4's
//                  RegulatoryInferenceRules, not supplied by the caller.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.RegulatoryInterpretation.Abstractions.Interpretation;

/// <summary>The input a regulatory-interpretation session reasons over.</summary>
public sealed record RegulatoryInterpretationInput(
    HazardType HazardType,
    SeverityLevel SeverityLevel);
