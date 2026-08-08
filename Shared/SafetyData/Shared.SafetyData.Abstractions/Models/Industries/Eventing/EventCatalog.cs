// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           EventCatalog.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic, in-memory catalog of the 21 event types
//                  PlantSafety recognizes. Every entry is built from a single
//                  minimal (SafetyFlag?, Regulator) source pairing --
//                  RelatedHazards/RelatedPrograms/RequiredSOPs/
//                  RequiredTraining/RequiredPermits are all computed from
//                  that pairing via the existing HazardCategoryCatalog/
//                  ProgramCatalog/ProgramSopRequirements/
//                  ProgramTrainingRequirements/ProgramPermitRequirements --
//                  never hand-authored per event.
//
//                  Severity/Category are the only fields that cannot be
//                  mechanically derived from existing enum data (there is no
//                  "event severity" concept anywhere upstream to compute
//                  from) -- these are a documented, reasoned editorial
//                  judgment, listed per event below, and should be treated as
//                  a defensible default classification, not a value PlantSafety's
//                  real AccidentManagement/SafetyEvents domains are known to
//                  assign (this subsystem does not read from those domains --
//                  see EventDefinition.cs's ARCHITECTURAL SCOPE note).
//
//                  Source table (EventCode : Flag : Regulator : Category : Severity):
//                    ChemicalSpill               : Chemicals            : EPA       : Environmental : High
//                    NearMiss                    : (none)               : OSHA      : Safety        : Low
//                    Injury                      : (none)               : OSHA      : Safety        : High
//                    Fire                        : Flammables           : NFPA      : Safety        : Critical
//                    ExplosionRisk               : ExplosionRisk        : ATF       : Safety        : Critical
//                    EquipmentFailure             : Machinery            : OSHA      : Safety        : Medium
//                    EnvironmentalRelease         : EnvironmentalPermits : EPA       : Environmental : High
//                    DotHazmatIncident            : HazmatShipping       : PHMSA     : Regulatory    : High
//                    ConfinedSpaceEntry           : ConfinedSpaces       : OSHA      : Safety        : High
//                    LotoViolation                : LOTO                 : OSHA      : Safety        : High
//                    HotWorkDeviation             : Flammables           : NFPA      : Safety        : Medium
//                    FleetIncident                : Fleet                : FMCSA     : Safety        : Medium
//                    OshaRecordable               : (none)               : OSHA      : Regulatory    : Medium
//                    RcraWasteIncident            : (none)               : EPA_RCRA  : Environmental : High
//                    TscaViolation                : (none)               : EPA_TSCA  : Regulatory    : Medium
//                    PhmsaShippingError           : (none)               : PHMSA     : Regulatory    : Medium
//                    NfpaFireProtectionFailure    : (none)               : NFPA      : Safety        : High
//                    AtfExplosiveMaterialDeviation: (none)               : ATF       : Regulatory    : Critical
//                    MshaMiningIncident           : (none)               : MSHA      : Safety        : High
//                    UscgWaterfrontIncident       : (none)               : USCG      : Safety        : High
//                    DhsCfatsSecurityDeviation    : (none)               : DHS_CFATS : Security      : Critical
//                  "(none)" flag entries are intentionally generic -- e.g. a
//                  near miss or injury is not tied to one hazard type at the
//                  catalog level; RelatedHazards/RelatedPrograms/RequiredSOPs/
//                  RequiredTraining/RequiredPermits are correctly empty for
//                  those until a real occurrence is further classified by
//                  the calling event domain.
//                  Additive-only evolution -- existing entries must never be
//                  renamed or have their Flag/Regulator/Severity/Category
//                  changed; new entries may only be added.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>
/// Deterministic, read-only catalog of every event type PlantSafety recognizes.
/// </summary>
public static class EventCatalog
{
    /// <summary>Every event type, in the order documented in this file's header.</summary>
    public static IReadOnlyList<EventDefinition> All { get; } = new[]
    {
        Build("ChemicalSpill", "Chemical Spill", "An uncontrolled release of a chemical substance.", EventSeverity.High, EventCategory.Environmental, SafetyFlag.Chemicals, Regulator.EPA),
        Build("NearMiss", "Near Miss", "An event that could have resulted in injury, illness, or damage but did not.", EventSeverity.Low, EventCategory.Safety, null, Regulator.OSHA),
        Build("Injury", "Injury", "A workplace event resulting in worker injury or illness.", EventSeverity.High, EventCategory.Safety, null, Regulator.OSHA),
        Build("Fire", "Fire", "An uncontrolled combustion event.", EventSeverity.Critical, EventCategory.Safety, SafetyFlag.Flammables, Regulator.NFPA),
        Build("ExplosionRisk", "Explosion Risk", "A condition or event presenting an imminent explosion hazard.", EventSeverity.Critical, EventCategory.Safety, SafetyFlag.ExplosionRisk, Regulator.ATF),
        Build("EquipmentFailure", "Equipment Failure", "An unplanned failure of machinery or equipment.", EventSeverity.Medium, EventCategory.Safety, SafetyFlag.Machinery, Regulator.OSHA),
        Build("EnvironmentalRelease", "Environmental Release", "A release of a regulated substance to air, water, or soil.", EventSeverity.High, EventCategory.Environmental, SafetyFlag.EnvironmentalPermits, Regulator.EPA),
        Build("DotHazmatIncident", "DOT Hazmat Incident", "A hazardous-materials transportation incident subject to DOT/PHMSA reporting.", EventSeverity.High, EventCategory.Regulatory, SafetyFlag.HazmatShipping, Regulator.PHMSA),
        Build("ConfinedSpaceEntry", "Confined Space Entry Event", "An event during or related to permit-required confined space entry.", EventSeverity.High, EventCategory.Safety, SafetyFlag.ConfinedSpaces, Regulator.OSHA),
        Build("LotoViolation", "LOTO Violation", "A failure to follow lockout/tagout energy-control procedures.", EventSeverity.High, EventCategory.Safety, SafetyFlag.LOTO, Regulator.OSHA),
        Build("HotWorkDeviation", "Hot Work Deviation", "A deviation from hot work permit requirements during welding, cutting, or similar work.", EventSeverity.Medium, EventCategory.Safety, SafetyFlag.Flammables, Regulator.NFPA),
        Build("FleetIncident", "Fleet Incident", "A vehicle incident involving the facility's fleet.", EventSeverity.Medium, EventCategory.Safety, SafetyFlag.Fleet, Regulator.FMCSA),
        Build("OshaRecordable", "OSHA Recordable Event", "A workplace event meeting OSHA recordkeeping criteria.", EventSeverity.Medium, EventCategory.Regulatory, null, Regulator.OSHA),
        Build("RcraWasteIncident", "RCRA Waste Incident", "An incident involving hazardous waste generation, storage, or disposal subject to RCRA.", EventSeverity.High, EventCategory.Environmental, null, Regulator.EPA_RCRA),
        Build("TscaViolation", "TSCA Violation", "A violation of Toxic Substances Control Act reporting or use requirements.", EventSeverity.Medium, EventCategory.Regulatory, null, Regulator.EPA_TSCA),
        Build("PhmsaShippingError", "PHMSA Shipping Error", "A hazardous-materials shipping documentation, packaging, or placarding error.", EventSeverity.Medium, EventCategory.Regulatory, null, Regulator.PHMSA),
        Build("NfpaFireProtectionFailure", "NFPA Fire Protection Failure", "A failure of a fire protection system or fire code requirement.", EventSeverity.High, EventCategory.Safety, null, Regulator.NFPA),
        Build("AtfExplosiveMaterialDeviation", "ATF Explosive Material Deviation", "A deviation from ATF explosives/ammunition manufacturing, storage, or licensing requirements.", EventSeverity.Critical, EventCategory.Regulatory, null, Regulator.ATF),
        Build("MshaMiningIncident", "MSHA Mining Incident", "A mining safety incident subject to MSHA jurisdiction.", EventSeverity.High, EventCategory.Safety, null, Regulator.MSHA),
        Build("UscgWaterfrontIncident", "USCG Waterfront Incident", "A waterfront or bulk liquid transfer incident subject to USCG jurisdiction.", EventSeverity.High, EventCategory.Safety, null, Regulator.USCG),
        Build("DhsCfatsSecurityDeviation", "DHS-CFATS Security Deviation", "A deviation from a Chemical Facility Anti-Terrorism Standards site security plan.", EventSeverity.Critical, EventCategory.Security, null, Regulator.DHS_CFATS),
    };

    /// <summary>Looks up an event type by its exact, canonical code.</summary>
    public static EventDefinition? FindByCode(string eventCode) =>
        All.FirstOrDefault(eventDefinition => string.Equals(eventDefinition.EventCode, eventCode, StringComparison.Ordinal));

    /// <summary>Every event type touching the given regulator, in catalog order.</summary>
    public static IReadOnlyList<EventDefinition> ForRegulator(Regulator regulator) =>
        All.Where(eventDefinition => eventDefinition.RelatedRegulators.Contains(regulator)).ToArray();

    /// <summary>Every event type in the given category, in catalog order.</summary>
    public static IReadOnlyList<EventDefinition> ForCategory(EventCategory category) =>
        All.Where(eventDefinition => eventDefinition.Category == category).ToArray();

    private static EventDefinition Build(
        string eventCode,
        string title,
        string description,
        EventSeverity severity,
        EventCategory category,
        SafetyFlag? flag,
        Regulator regulator)
    {
        IReadOnlyList<HazardCategory> relatedHazards = flag is SafetyFlag f
            ? new[] { HazardCategoryCatalog.ForFlag(f) }
            : Array.Empty<HazardCategory>();

        IReadOnlyList<ProgramDefinition> relatedPrograms = flag is SafetyFlag flagForPrograms
            ? ProgramCatalog.ForFlag(flagForPrograms)
            : Array.Empty<ProgramDefinition>();

        IReadOnlyList<SopTemplate> requiredSops = relatedPrograms.Select(ProgramSopRequirements.For).ToArray();
        IReadOnlyList<TrainingRequirement> requiredTraining = relatedPrograms.Select(ProgramTrainingRequirements.For).ToArray();
        IReadOnlyList<PermitType> requiredPermits = relatedPrograms.Select(ProgramPermitRequirements.For).Distinct().ToArray();

        return new EventDefinition(
            EventCode: eventCode,
            Title: title,
            Description: description,
            Severity: severity,
            Category: category,
            RelatedHazards: relatedHazards,
            RelatedPrograms: relatedPrograms,
            RelatedRegulators: new[] { regulator },
            RequiredSops: requiredSops,
            RequiredTraining: requiredTraining,
            RequiredPermits: requiredPermits);
    }
}
