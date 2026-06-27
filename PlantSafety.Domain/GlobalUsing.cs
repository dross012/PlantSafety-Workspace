// ============================================================================
//  Project:        PlantSafety.Platform
//  Layer:          Domain
//  File:           GlobalUsing.cs
//  Created:        2026-05-26
//  Updated:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Project-wide global using directives for PlantSafety.Domain.
//      Covers SharedKernel primitives, Common abstractions, and all module
//      namespaces so individual files need no explicit using statements.
//      Additive-only evolution.
// ============================================================================

// ---- System ----------------------------------------------------------------
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;

// ---- SharedKernel ----------------------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;

// ---- Common: Abstractions --------------------------------------------------
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Common.Base;
global using PlantSafety.Domain.Common.Errors;
global using PlantSafety.Domain.Common.Exceptions;
global using PlantSafety.Domain.Common.Rules;
global using PlantSafety.Domain.Common.Services;
global using PlantSafety.Domain.Common.Types.Enums;
global using PlantSafety.Domain.Common.Types.Lookups;
global using PlantSafety.Domain.Common.Types.StrongIds;

// ---- Operational.Alerts (canonical reference module) -----------------------
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Entities;
global using PlantSafety.Domain.Operational.Alerts.Enums;
global using PlantSafety.Domain.Operational.Alerts.Events;
global using PlantSafety.Domain.Operational.Alerts.Exceptions;
global using PlantSafety.Domain.Operational.Alerts.Rules;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

// ---- Analytics -------------------------------------------------------------
global using PlantSafety.Domain.Analytics.Culture.Aggregates;
global using PlantSafety.Domain.Analytics.Culture.Enums;
global using PlantSafety.Domain.Analytics.Culture.Events;
global using PlantSafety.Domain.Analytics.Culture.ValueObjects;
global using PlantSafety.Domain.Analytics.Dashboard.Aggregates;
global using PlantSafety.Domain.Analytics.Dashboard.Enums;
global using PlantSafety.Domain.Analytics.Dashboard.Events;
global using PlantSafety.Domain.Analytics.Dashboard.ValueObjects;
global using PlantSafety.Domain.Analytics.Engagement.Aggregates;
global using PlantSafety.Domain.Analytics.Engagement.Events;
global using PlantSafety.Domain.Analytics.Engagement.ValueObjects;
global using PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates;
global using PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;
global using PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects;
global using PlantSafety.Domain.Analytics.ScoreCards.Aggregates;
global using PlantSafety.Domain.Analytics.ScoreCards.Enums;
global using PlantSafety.Domain.Analytics.ScoreCards.Events;
global using PlantSafety.Domain.Analytics.ScoreCards.ValueObjects;

// ---- Compliance ------------------------------------------------------------
global using PlantSafety.Domain.Compliance.Auditing.Aggregates;
global using PlantSafety.Domain.Compliance.Auditing.Enums;
global using PlantSafety.Domain.Compliance.Auditing.Events;
global using PlantSafety.Domain.Compliance.Auditing.ValueObjects;
global using PlantSafety.Domain.Compliance.Certification.Aggregates;
global using PlantSafety.Domain.Compliance.Certification.Enums;
global using PlantSafety.Domain.Compliance.Certification.Events;
global using PlantSafety.Domain.Compliance.Certification.ValueObjects;
global using PlantSafety.Domain.Compliance.Compliance.Aggregates;
global using PlantSafety.Domain.Compliance.Compliance.Enums;
global using PlantSafety.Domain.Compliance.Compliance.Events;
global using PlantSafety.Domain.Compliance.Compliance.ValueObjects;
global using PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates;
global using PlantSafety.Domain.Compliance.ManagementOfChange.Events;
global using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects;
global using PlantSafety.Domain.Compliance.Permits.Aggregates;
global using PlantSafety.Domain.Compliance.Permits.Enums;
global using PlantSafety.Domain.Compliance.Permits.Events;
global using PlantSafety.Domain.Compliance.Permits.ValueObjects;
global using PlantSafety.Domain.Compliance.Policy.Aggregates;
global using PlantSafety.Domain.Compliance.Policy.Enums;
global using PlantSafety.Domain.Compliance.Policy.Events;
global using PlantSafety.Domain.Compliance.Policy.ValueObjects;
global using PlantSafety.Domain.Compliance.Procedures.Aggregates;
global using PlantSafety.Domain.Compliance.Procedures.Events;
global using PlantSafety.Domain.Compliance.Procedures.ValueObjects;
global using PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates;
global using PlantSafety.Domain.Compliance.RegulatoryCalendar.Events;
global using PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects;

// ---- Operational -----------------------------------------------------------
global using PlantSafety.Domain.Operational.Checklists.Aggregates;
global using PlantSafety.Domain.Operational.Checklists.Enums;
global using PlantSafety.Domain.Operational.Checklists.Events;
global using PlantSafety.Domain.Operational.Checklists.ValueObjects;
global using PlantSafety.Domain.Operational.Contractor.Aggregates;
global using PlantSafety.Domain.Operational.Contractor.Enums;
global using PlantSafety.Domain.Operational.Contractor.Events;
global using PlantSafety.Domain.Operational.Contractor.ValueObjects;
global using PlantSafety.Domain.Operational.PPEManagement.Aggregates;
global using PlantSafety.Domain.Operational.PPEManagement.Enums;
global using PlantSafety.Domain.Operational.PPEManagement.Events;
global using PlantSafety.Domain.Operational.PPEManagement.ValueObjects;
global using PlantSafety.Domain.Operational.RiskManagement.Aggregates;
global using PlantSafety.Domain.Operational.RiskManagement.Enums;
global using PlantSafety.Domain.Operational.RiskManagement.Events;
global using PlantSafety.Domain.Operational.RiskManagement.ValueObjects;
global using PlantSafety.Domain.Operational.Schedules.Aggregates;
global using PlantSafety.Domain.Operational.Schedules.Enums;
global using PlantSafety.Domain.Operational.Schedules.Events;
global using PlantSafety.Domain.Operational.Schedules.ValueObjects;
global using PlantSafety.Domain.Operational.Training.Aggregates;
global using PlantSafety.Domain.Operational.Training.Enums;
global using PlantSafety.Domain.Operational.Training.Events;
global using PlantSafety.Domain.Operational.Training.ValueObjects;

// ---- SafetyEvents ----------------------------------------------------------
global using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates;
global using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Enums;
global using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
global using PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects;
global using PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;
global using PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;
global using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
global using PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects;
global using PlantSafety.Domain.SafetyEvents.Observation.Aggregates;
global using PlantSafety.Domain.SafetyEvents.Observation.Enums;
global using PlantSafety.Domain.SafetyEvents.Observation.Events;
global using PlantSafety.Domain.SafetyEvents.Observation.ValueObjects;

// ---- Slim modules ----------------------------------------------------------
global using PlantSafety.Domain.AiGovernance.Aggregates;
global using PlantSafety.Domain.AiGovernance.Events;
global using PlantSafety.Domain.AiGovernance.ValueObjects;
global using PlantSafety.Domain.AiSafetyIntelligence.Aggregates;
global using PlantSafety.Domain.AiSafetyIntelligence.Events;
global using PlantSafety.Domain.AiSafetyIntelligence.ValueObjects;
global using PlantSafety.Domain.ContractorVisitor.Aggregates;
global using PlantSafety.Domain.ContractorVisitor.Events;
global using PlantSafety.Domain.ContractorVisitor.ValueObjects;
global using PlantSafety.Domain.DigitalTwin.Aggregates;
global using PlantSafety.Domain.DigitalTwin.Events;
global using PlantSafety.Domain.DigitalTwin.ValueObjects;
global using PlantSafety.Domain.EnterpriseFabric.Aggregates;
global using PlantSafety.Domain.EnterpriseFabric.Events;
global using PlantSafety.Domain.EnterpriseFabric.ValueObjects;
global using PlantSafety.Domain.EnvironmentalHealth.Aggregates;
global using PlantSafety.Domain.EnvironmentalHealth.Events;
global using PlantSafety.Domain.EnvironmentalHealth.ValueObjects;
global using PlantSafety.Domain.Governance.Aggregates;
global using PlantSafety.Domain.Governance.Events;
global using PlantSafety.Domain.Governance.ValueObjects;
global using PlantSafety.Domain.HumanFactors.Aggregates;
global using PlantSafety.Domain.HumanFactors.Events;
global using PlantSafety.Domain.HumanFactors.ValueObjects;
global using PlantSafety.Domain.OperationalSafety.Aggregates;
global using PlantSafety.Domain.OperationalSafety.Events;
global using PlantSafety.Domain.OperationalSafety.ValueObjects;
global using PlantSafety.Domain.Sustainability.Aggregates;
global using PlantSafety.Domain.Sustainability.Events;
global using PlantSafety.Domain.Sustainability.ValueObjects;
global using PlantSafety.Domain.Training.Aggregates;
global using PlantSafety.Domain.Training.Events;
global using PlantSafety.Domain.Training.ValueObjects;
global using PlantSafety.Domain.WorkerWellBeing.Aggregates;
global using PlantSafety.Domain.WorkerWellBeing.Events;
global using PlantSafety.Domain.WorkerWellBeing.ValueObjects;

// ---- Financials / PlanningProjects -----------------------------------------
global using PlantSafety.Domain.Financials.Entities;
global using PlantSafety.Domain.Financials.Enums;
global using PlantSafety.Domain.Financials.Events;
global using PlantSafety.Domain.Financials.ValueObjects;
global using PlantSafety.Domain.PlanningProjects.Entities;
global using PlantSafety.Domain.PlanningProjects.Enums;
global using PlantSafety.Domain.PlanningProjects.Events;
global using PlantSafety.Domain.PlanningProjects.ValueObjects;
