// ============================================================================
//  Project:        PlantSafety.Platform
//  Layer:          Infrastructure
//  File:           GlobalUsing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Project-wide global using directives for PlantSafety.Infrastructure.
//      Covers System, Dapper, MassTransit, SharedKernel, Domain, and
//      Application namespaces so individual files need no explicit usings.
//      Additive-only evolution.
// ============================================================================

// ---- System ----------------------------------------------------------------
global using System.Data;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Collections.ObjectModel;

// ---- Microsoft -------------------------------------------------------------
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Data.SqlClient;

// ---- Dapper ----------------------------------------------------------------
global using Dapper;

// ---- MassTransit -----------------------------------------------------------
global using MassTransit;

// ---- SharedKernel ----------------------------------------------------------
global using PlantSafety.SharedKernel.Identifiers;
global using PlantSafety.SharedKernel.Primitives.ValueTypes;

// ---- Domain: Common --------------------------------------------------------
global using PlantSafety.Domain.Common.Abstractions;
global using PlantSafety.Domain.Common.Types.StrongIds;

// ---- Domain: Operational.Alerts (canonical reference module) ---------------
global using PlantSafety.Domain.Operational.Alerts.Aggregates;
global using PlantSafety.Domain.Operational.Alerts.Events;
global using PlantSafety.Domain.Operational.Alerts.ValueObjects.Ids;

// ---- Domain: module Events (integration-event / consumer support) ----------
global using PlantSafety.Domain.Analytics.Culture.Events;
global using PlantSafety.Domain.Analytics.Dashboard.Events;
global using PlantSafety.Domain.Analytics.Engagement.Events;
global using PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;
global using PlantSafety.Domain.Analytics.ScoreCards.Events;
global using PlantSafety.Domain.Compliance.Auditing.Events;
global using PlantSafety.Domain.Compliance.Certification.Events;
global using PlantSafety.Domain.Compliance.Compliance.Events;
global using PlantSafety.Domain.Compliance.ManagementOfChange.Events;
global using PlantSafety.Domain.Compliance.Permits.Events;
global using PlantSafety.Domain.Compliance.Policy.Events;
global using PlantSafety.Domain.Compliance.Procedures.Events;
global using PlantSafety.Domain.Compliance.RegulatoryCalendar.Events;
global using PlantSafety.Domain.Operational.Checklists.Events;
global using PlantSafety.Domain.Operational.PPEManagement.Events;
global using PlantSafety.Domain.Operational.RiskManagement.Events;
global using PlantSafety.Domain.Operational.Schedules.Events;
global using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
global using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
global using PlantSafety.Domain.SafetyEvents.Observation.Events;

// ---- Infrastructure: Common ------------------------------------------------
global using PlantSafety.Infrastructure.Common.Configuration;
global using PlantSafety.Infrastructure.Common.Constants;
global using PlantSafety.Infrastructure.Common.Exceptions;
global using PlantSafety.Infrastructure.Common.Pipelines;
global using PlantSafety.Infrastructure.Common.Serialization;
global using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
global using PlantSafety.Infrastructure.Persistence.Dapper.Commands;
global using PlantSafety.Infrastructure.Persistence.Dapper.Queries;
global using PlantSafety.Infrastructure.Persistence.Dapper.SqlBuilder;
global using PlantSafety.Infrastructure.Persistence.UnitOfWork;

// ---- Infrastructure: Messaging ---------------------------------------------
global using PlantSafety.Infrastructure.Messaging.Consumers;

// ---- Infrastructure: OperationalSafety.Alerts (canonical reference module) -
global using PlantSafety.Infrastructure.OperationalSafety.Alerts.Interfaces;
global using PlantSafety.Infrastructure.OperationalSafety.Alerts.Mapping;
global using PlantSafety.Infrastructure.OperationalSafety.Alerts.Messaging;
global using PlantSafety.Infrastructure.OperationalSafety.Alerts.Persistence;
global using PlantSafety.Infrastructure.OperationalSafety.Alerts.Repositories;

// ---- Infrastructure: OperationalSafety.Checklists ---------------------------
global using PlantSafety.Infrastructure.OperationalSafety.Checklists.Interfaces;
global using PlantSafety.Infrastructure.OperationalSafety.Checklists.Mapping;
global using PlantSafety.Infrastructure.OperationalSafety.Checklists.Messaging;
global using PlantSafety.Infrastructure.OperationalSafety.Checklists.Persistence;
global using PlantSafety.Infrastructure.OperationalSafety.Checklists.Repositories;

// ---- Infrastructure: OperationalSafety.PPEManagement ------------------------
global using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Interfaces;
global using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Mapping;
global using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Messaging;
global using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Persistence;
global using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Repositories;

// ---- Infrastructure: OperationalSafety.RiskManagement ------------------------
global using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Interfaces;
global using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Mapping;
global using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Messaging;
global using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Persistence;
global using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Repositories;

// ---- Infrastructure: OperationalSafety.Schedules -----------------------------
global using PlantSafety.Infrastructure.OperationalSafety.Schedules.Interfaces;
global using PlantSafety.Infrastructure.OperationalSafety.Schedules.Mapping;
global using PlantSafety.Infrastructure.OperationalSafety.Schedules.Messaging;
global using PlantSafety.Infrastructure.OperationalSafety.Schedules.Persistence;
global using PlantSafety.Infrastructure.OperationalSafety.Schedules.Repositories;

// ---- Infrastructure: Analytics ----------------------------------------------
global using PlantSafety.Infrastructure.Analytics.Culture.Messaging;
global using PlantSafety.Infrastructure.Analytics.Culture.Persistence;
global using PlantSafety.Infrastructure.Analytics.Dashboard.Messaging;
global using PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;
global using PlantSafety.Infrastructure.Analytics.Engagement.Messaging;
global using PlantSafety.Infrastructure.Analytics.Engagement.Persistence;
global using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Messaging;
global using PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;
global using PlantSafety.Infrastructure.Analytics.ScoreCards.Messaging;
global using PlantSafety.Infrastructure.Analytics.ScoreCards.Persistence;

// ---- Infrastructure: Compliance ---------------------------------------------
global using PlantSafety.Infrastructure.Compliance.Auditing.Messaging;
global using PlantSafety.Infrastructure.Compliance.Auditing.Persistence;
global using PlantSafety.Infrastructure.Compliance.Certification.Messaging;
global using PlantSafety.Infrastructure.Compliance.Certification.Persistence;
global using PlantSafety.Infrastructure.Compliance.Compliance.Messaging;
global using PlantSafety.Infrastructure.Compliance.Compliance.Persistence;
global using PlantSafety.Infrastructure.Compliance.ManagementOfChange.Messaging;
global using PlantSafety.Infrastructure.Compliance.ManagementOfChange.Persistence;
global using PlantSafety.Infrastructure.Compliance.Permits.Messaging;
global using PlantSafety.Infrastructure.Compliance.Permits.Persistence;
global using PlantSafety.Infrastructure.Compliance.Policies.Messaging;
global using PlantSafety.Infrastructure.Compliance.Policies.Persistence;
global using PlantSafety.Infrastructure.Compliance.Procedures.Messaging;
global using PlantSafety.Infrastructure.Compliance.Procedures.Persistence;
global using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Messaging;
global using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Persistence;

// ---- Infrastructure: SafetyEvents --------------------------------------------
global using PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Messaging;
global using PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;
global using PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Messaging;
global using PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Persistence;
global using PlantSafety.Infrastructure.SafetyEvents.Observation.Messaging;
global using PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;

// ---- Infrastructure: Training.CompetencyManagement ---------------------------
global using PlantSafety.Infrastructure.Training.CompetencyManagement.Interfaces;
global using PlantSafety.Infrastructure.Training.CompetencyManagement.Mapping;
global using PlantSafety.Infrastructure.Training.CompetencyManagement.Messaging;
global using PlantSafety.Infrastructure.Training.CompetencyManagement.Persistence;
global using PlantSafety.Infrastructure.Training.CompetencyManagement.Repositories;

// ---- Infrastructure: Financials -----------------------------------------------
global using PlantSafety.Infrastructure.Financials.Persistence;

// ---- Infrastructure: PlanningProjects ------------------------------------------
global using PlantSafety.Infrastructure.PlanningProjects.Persistence;
