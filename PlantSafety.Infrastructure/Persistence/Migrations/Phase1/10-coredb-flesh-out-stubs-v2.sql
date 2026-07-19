USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 1 (v2) -- Table-Fleshing Re-run
-- Script:        10-coredb-flesh-out-stubs-v2.sql
-- Purpose:       Add the real business-property columns that the
--                modular-creation-agent grounded into the 22 previously-stub
--                PlantSafety.Domain aggregates (and their *HistoryEntry
--                companions) on 2026-07-18, now that Auditing / Certification /
--                Compliance / ManagementOfChange / Permits / Policy /
--                Procedures / RegulatoryCalendar (Compliance schema);
--                Checklists / Contractor / PPEManagement / RiskManagement /
--                Schedules / Training (Operational schema); EmergencyManagement
--                / IncidentManagement / Observation (SafetyEvents schema); and
--                Culture / Dashboard / Engagement / PredictiveAnalytics /
--                ScoreCards (Analytics schema) carry real domain state instead
--                of the identity-only scaffold shape.
--
-- Grounding:     Every column below is taken verbatim from the live C# source
--                (property name + CLR type) of each aggregate's Aggregates/*.cs
--                and matching Entities/*HistoryEntry.cs, read directly for this
--                pass -- not invented, not carried over from memory. No
--                @TenantId parameter is used (this is the CoreDB
--                database-per-solution TenantId-COLUMN model already
--                established in the live schema, per
--                sql-database-skill/references/00-canonical-templates.md).
--
-- Type mapping conventions used (matching live CoreDB Phase 1/2 conventions):
--   StrongId / Guid / Guid?              -> uniqueidentifier [NOT] NULL
--   UtcTimestamp / UtcTimestamp?         -> datetime2(7) [NOT] NULL
--   string (non-nullable, .Trim()'d)     -> nvarchar(n) NOT NULL, DEFAULT
--                                          literal matches the C# Create()
--                                          parameter default when one exists
--                                          in the factory method signature
--   string? (nullable, history snapshot) -> nvarchar(n) NULL, no default
--   int / int?                           -> int [NOT] NULL, DEFAULT matches
--                                          the C# Create() default when present
--   bool                                 -> bit NOT NULL, DEFAULT matches the
--                                          C# Create() default when present
--   decimal / decimal? "0-100" score     -> decimal(7,4). No column in this
--                                          batch has an explicit F4/bounded
--                                          [0,1] contract like PredictiveDB's
--                                          ConfidenceScore (Phase 2,
--                                          21-float-to-decimal.sql, DECIMAL(5,4)
--                                          for [0,1]); these are all documented
--                                          "(0-100)" percentages/indices with no
--                                          stated decimal-place requirement, so
--                                          DECIMAL(7,4) (up to 999.9999) is used
--                                          as a judgment call that (a) keeps the
--                                          same 4-decimal-place fixed-point
--                                          convention as Phase 2, and (b)
--                                          comfortably covers the full 0-100
--                                          range with headroom. This is an
--                                          engineering-precision choice, not a
--                                          domain-semantics guess.
--
-- NOT included in this pass (see report):
--   - Operational.RiskManagement.RiskScore: a get-only COMPUTED C# property
--     (LikelihoodRating * SeverityRating), no private setter/backing field --
--     not persisted domain state on the aggregate itself. The *explicit*
--     RiskScoreAtTransition snapshot on RiskManagementHistoryEntry (a real
--     stored int property) IS added below.
--   - No new CHECK constraints are added in this pass. Every module was
--     re-verified via `find .../Enums/*.cs` and only ONE enum exists per
--     module -- the shared lifecycle Status enum already covered by the
--     Phase 1 CK_<Schema>_<Table>_Status constraint. All other "type/
--     category/status/severity/rating" string properties found in this pass
--     (e.g. AuditType, RiskLevel, Severity, QualificationStatus,
--     ComplianceStatus, ObservationType, InjuryType, ApprovalStatus,
--     RiskCategory, DashboardType, KpiCategory, PredictionType,
--     ParticipationChannel, ComplianceCategory, RecurrencePattern,
--     ChecklistType, PpeType, EmergencyType, HazardCategory) are plain
--     C# strings with NO backing enum type anywhere in their module -- adding
--     a CHECK constraint for these would mean inventing an allowed-value list
--     from nothing, which the No-Guessing Rule forbids. Flagged, not guessed.
--
-- Idempotent:    Every ADD is guarded by COL_LENGTH(...) IS NULL; safe to
--                re-run. All 44 target tables (22 aggregates + 22
--                HistoryEntry companions) were verified at 0 rows before
--                this script ran, so NOT NULL adds with/without DEFAULT are
--                both safe; DEFAULTs are still included where the C# source
--                states one, for forward-compatible documentation and so the
--                column also behaves correctly if this script is ever run
--                against a table that has since gained rows via normal app
--                traffic.
-- =============================================

------------------------------------------------------------------------------
-- Compliance.Auditing / Compliance.AuditingHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Auditing', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Auditing', 'AuditTitle') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [AuditTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_Auditing_AuditTitle DEFAULT (N'');
IF COL_LENGTH('Compliance.Auditing', 'AuditType') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [AuditType] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Auditing_AuditType DEFAULT (N'Internal');
IF COL_LENGTH('Compliance.Auditing', 'Scope') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [Scope] nvarchar(max) NOT NULL CONSTRAINT DF_Compliance_Auditing_Scope DEFAULT (N'');
IF COL_LENGTH('Compliance.Auditing', 'AuditorId') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [AuditorId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Auditing_AuditorId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Auditing', 'ScheduledAtUtc') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [ScheduledAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Auditing', 'CompletedAtUtc') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [CompletedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Auditing', 'FindingsCount') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [FindingsCount] int NOT NULL CONSTRAINT DF_Compliance_Auditing_FindingsCount DEFAULT (0);
IF COL_LENGTH('Compliance.Auditing', 'Score') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [Score] decimal(7,4) NULL;
IF COL_LENGTH('Compliance.Auditing', 'ComplianceRating') IS NULL
    ALTER TABLE [Compliance].[Auditing] ADD [ComplianceRating] nvarchar(50) NOT NULL CONSTRAINT DF_Compliance_Auditing_ComplianceRating DEFAULT (N'NotRated');
GO

IF COL_LENGTH('Compliance.AuditingHistoryEntry', 'FindingsCountAtTransition') IS NULL
    ALTER TABLE [Compliance].[AuditingHistoryEntry] ADD [FindingsCountAtTransition] int NOT NULL CONSTRAINT DF_Compliance_AuditingHistoryEntry_FindingsCountAtTransition DEFAULT (0);
IF COL_LENGTH('Compliance.AuditingHistoryEntry', 'ScoreAtTransition') IS NULL
    ALTER TABLE [Compliance].[AuditingHistoryEntry] ADD [ScoreAtTransition] decimal(7,4) NULL;
IF COL_LENGTH('Compliance.AuditingHistoryEntry', 'ComplianceRatingAtTransition') IS NULL
    ALTER TABLE [Compliance].[AuditingHistoryEntry] ADD [ComplianceRatingAtTransition] nvarchar(50) NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Certification / Compliance.CertificationHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Certification', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Certification', 'HolderWorkerId') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [HolderWorkerId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Certification_HolderWorkerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Certification', 'CertificationName') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [CertificationName] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_Certification_CertificationName DEFAULT (N'');
IF COL_LENGTH('Compliance.Certification', 'CertificationBody') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [CertificationBody] nvarchar(150) NOT NULL CONSTRAINT DF_Compliance_Certification_CertificationBody DEFAULT (N'');
IF COL_LENGTH('Compliance.Certification', 'CertificationNumber') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [CertificationNumber] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Certification_CertificationNumber DEFAULT (N'');
IF COL_LENGTH('Compliance.Certification', 'IssuedAtUtc') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [IssuedAtUtc] datetime2(7) NOT NULL CONSTRAINT DF_Compliance_Certification_IssuedAtUtc DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('Compliance.Certification', 'ExpiresAtUtc') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [ExpiresAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Certification', 'RenewalRequired') IS NULL
    ALTER TABLE [Compliance].[Certification] ADD [RenewalRequired] bit NOT NULL CONSTRAINT DF_Compliance_Certification_RenewalRequired DEFAULT (1);
GO

IF COL_LENGTH('Compliance.CertificationHistoryEntry', 'ExpiresAtUtcAtTransition') IS NULL
    ALTER TABLE [Compliance].[CertificationHistoryEntry] ADD [ExpiresAtUtcAtTransition] datetime2(7) NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Compliance / Compliance.ComplianceHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Compliance', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Compliance', 'RegulationReference') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [RegulationReference] nvarchar(200) NOT NULL CONSTRAINT DF_Compliance_Compliance_RegulationReference DEFAULT (N'');
IF COL_LENGTH('Compliance.Compliance', 'RuleDescription') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [RuleDescription] nvarchar(max) NOT NULL CONSTRAINT DF_Compliance_Compliance_RuleDescription DEFAULT (N'');
IF COL_LENGTH('Compliance.Compliance', 'ComplianceScore') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [ComplianceScore] decimal(7,4) NOT NULL CONSTRAINT DF_Compliance_Compliance_ComplianceScore DEFAULT (0);
IF COL_LENGTH('Compliance.Compliance', 'NonConformanceCount') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [NonConformanceCount] int NOT NULL CONSTRAINT DF_Compliance_Compliance_NonConformanceCount DEFAULT (0);
IF COL_LENGTH('Compliance.Compliance', 'ResponsiblePartyId') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [ResponsiblePartyId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Compliance_ResponsiblePartyId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Compliance', 'LastEvaluatedAtUtc') IS NULL
    ALTER TABLE [Compliance].[Compliance] ADD [LastEvaluatedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Compliance.ComplianceHistoryEntry', 'ComplianceScoreAtTransition') IS NULL
    ALTER TABLE [Compliance].[ComplianceHistoryEntry] ADD [ComplianceScoreAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Compliance_ComplianceHistoryEntry_ComplianceScoreAtTransition DEFAULT (0);
IF COL_LENGTH('Compliance.ComplianceHistoryEntry', 'NonConformanceCountAtTransition') IS NULL
    ALTER TABLE [Compliance].[ComplianceHistoryEntry] ADD [NonConformanceCountAtTransition] int NOT NULL CONSTRAINT DF_Compliance_ComplianceHistoryEntry_NonConformanceCountAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Compliance.ManagementOfChange / Compliance.ManagementOfChangeHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.ManagementOfChange', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.ManagementOfChange', 'ChangeTitle') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ChangeTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_ChangeTitle DEFAULT (N'');
IF COL_LENGTH('Compliance.ManagementOfChange', 'ChangeType') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ChangeType] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_ChangeType DEFAULT (N'Process');
IF COL_LENGTH('Compliance.ManagementOfChange', 'Justification') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [Justification] nvarchar(max) NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_Justification DEFAULT (N'');
IF COL_LENGTH('Compliance.ManagementOfChange', 'RiskLevel') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [RiskLevel] nvarchar(50) NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_RiskLevel DEFAULT (N'Low');
IF COL_LENGTH('Compliance.ManagementOfChange', 'RequestedByUserId') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [RequestedByUserId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_RequestedByUserId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.ManagementOfChange', 'ApprovedByUserId') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ApprovedByUserId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.ManagementOfChange', 'ApprovalStatus') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ApprovalStatus] nvarchar(50) NOT NULL CONSTRAINT DF_Compliance_ManagementOfChange_ApprovalStatus DEFAULT (N'Pending');
IF COL_LENGTH('Compliance.ManagementOfChange', 'ImplementationAtUtc') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ImplementationAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.ManagementOfChange', 'ReviewDueAtUtc') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChange] ADD [ReviewDueAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Compliance.ManagementOfChangeHistoryEntry', 'ApprovalStatusAtTransition') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChangeHistoryEntry] ADD [ApprovalStatusAtTransition] nvarchar(50) NULL;
IF COL_LENGTH('Compliance.ManagementOfChangeHistoryEntry', 'RiskLevelAtTransition') IS NULL
    ALTER TABLE [Compliance].[ManagementOfChangeHistoryEntry] ADD [RiskLevelAtTransition] nvarchar(50) NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Permits / Compliance.PermitsHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Permits', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Permits', 'PermitType') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [PermitType] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Permits_PermitType DEFAULT (N'');
IF COL_LENGTH('Compliance.Permits', 'PermitNumber') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [PermitNumber] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Permits_PermitNumber DEFAULT (N'');
IF COL_LENGTH('Compliance.Permits', 'WorkDescription') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [WorkDescription] nvarchar(max) NOT NULL CONSTRAINT DF_Compliance_Permits_WorkDescription DEFAULT (N'');
IF COL_LENGTH('Compliance.Permits', 'Location') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [Location] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_Permits_Location DEFAULT (N'');
IF COL_LENGTH('Compliance.Permits', 'HazardsIdentified') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [HazardsIdentified] nvarchar(max) NOT NULL CONSTRAINT DF_Compliance_Permits_HazardsIdentified DEFAULT (N'');
IF COL_LENGTH('Compliance.Permits', 'RequestedByUserId') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [RequestedByUserId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Permits_RequestedByUserId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Permits', 'ApprovedByUserId') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [ApprovedByUserId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Permits', 'IssuedAtUtc') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [IssuedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Permits', 'ExpiresAtUtc') IS NULL
    ALTER TABLE [Compliance].[Permits] ADD [ExpiresAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Compliance.PermitsHistoryEntry', 'ExpiresAtUtcAtTransition') IS NULL
    ALTER TABLE [Compliance].[PermitsHistoryEntry] ADD [ExpiresAtUtcAtTransition] datetime2(7) NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Policy / Compliance.PolicyHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Policy', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Policy', 'PolicyTitle') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [PolicyTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_Policy_PolicyTitle DEFAULT (N'');
IF COL_LENGTH('Compliance.Policy', 'PolicyVersion') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [PolicyVersion] int NOT NULL CONSTRAINT DF_Compliance_Policy_PolicyVersion DEFAULT (1);
IF COL_LENGTH('Compliance.Policy', 'Category') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [Category] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Policy_Category DEFAULT (N'');
IF COL_LENGTH('Compliance.Policy', 'OwnerId') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [OwnerId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Policy_OwnerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Policy', 'EffectiveAtUtc') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [EffectiveAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Policy', 'AcknowledgmentRequired') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [AcknowledgmentRequired] bit NOT NULL CONSTRAINT DF_Compliance_Policy_AcknowledgmentRequired DEFAULT (1);
IF COL_LENGTH('Compliance.Policy', 'AcknowledgmentCount') IS NULL
    ALTER TABLE [Compliance].[Policy] ADD [AcknowledgmentCount] int NOT NULL CONSTRAINT DF_Compliance_Policy_AcknowledgmentCount DEFAULT (0);
GO

IF COL_LENGTH('Compliance.PolicyHistoryEntry', 'PolicyVersionAtTransition') IS NULL
    ALTER TABLE [Compliance].[PolicyHistoryEntry] ADD [PolicyVersionAtTransition] int NOT NULL CONSTRAINT DF_Compliance_PolicyHistoryEntry_PolicyVersionAtTransition DEFAULT (1);
IF COL_LENGTH('Compliance.PolicyHistoryEntry', 'AcknowledgmentCountAtTransition') IS NULL
    ALTER TABLE [Compliance].[PolicyHistoryEntry] ADD [AcknowledgmentCountAtTransition] int NOT NULL CONSTRAINT DF_Compliance_PolicyHistoryEntry_AcknowledgmentCountAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Compliance.Procedures / Compliance.ProceduresHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.Procedures', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.Procedures', 'ProcedureTitle') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [ProcedureTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_Procedures_ProcedureTitle DEFAULT (N'');
IF COL_LENGTH('Compliance.Procedures', 'ProcedureNumber') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [ProcedureNumber] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Procedures_ProcedureNumber DEFAULT (N'');
IF COL_LENGTH('Compliance.Procedures', 'Category') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [Category] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_Procedures_Category DEFAULT (N'');
IF COL_LENGTH('Compliance.Procedures', 'RevisionNumber') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [RevisionNumber] int NOT NULL CONSTRAINT DF_Compliance_Procedures_RevisionNumber DEFAULT (1);
IF COL_LENGTH('Compliance.Procedures', 'OwnerId') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [OwnerId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_Procedures_OwnerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.Procedures', 'EffectiveAtUtc') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [EffectiveAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.Procedures', 'ReviewDueAtUtc') IS NULL
    ALTER TABLE [Compliance].[Procedures] ADD [ReviewDueAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Compliance.ProceduresHistoryEntry', 'RevisionNumberAtTransition') IS NULL
    ALTER TABLE [Compliance].[ProceduresHistoryEntry] ADD [RevisionNumberAtTransition] int NOT NULL CONSTRAINT DF_Compliance_ProceduresHistoryEntry_RevisionNumberAtTransition DEFAULT (1);
GO

------------------------------------------------------------------------------
-- Compliance.RegulatoryCalendar / Compliance.RegulatoryCalendarHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'SiteId') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'EventTitle') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [EventTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_EventTitle DEFAULT (N'');
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'RegulatoryBody') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [RegulatoryBody] nvarchar(150) NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_RegulatoryBody DEFAULT (N'');
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'JurisdictionCode') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [JurisdictionCode] nvarchar(50) NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_JurisdictionCode DEFAULT (N'');
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'ComplianceCategory') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [ComplianceCategory] nvarchar(100) NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_ComplianceCategory DEFAULT (N'');
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'ResponsiblePartyId') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [ResponsiblePartyId] uniqueidentifier NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_ResponsiblePartyId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'DueAtUtc') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [DueAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Compliance.RegulatoryCalendar', 'ReminderLeadDays') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD [ReminderLeadDays] int NOT NULL CONSTRAINT DF_Compliance_RegulatoryCalendar_ReminderLeadDays DEFAULT (30);
GO

IF COL_LENGTH('Compliance.RegulatoryCalendarHistoryEntry', 'DueAtUtcAtTransition') IS NULL
    ALTER TABLE [Compliance].[RegulatoryCalendarHistoryEntry] ADD [DueAtUtcAtTransition] datetime2(7) NULL;
GO

------------------------------------------------------------------------------
-- Operational.Checklists / Operational.ChecklistsHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.Checklists', 'SiteId') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.Checklists', 'ChecklistTitle') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [ChecklistTitle] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Checklists_ChecklistTitle DEFAULT (N'');
IF COL_LENGTH('Operational.Checklists', 'ChecklistType') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [ChecklistType] nvarchar(100) NOT NULL CONSTRAINT DF_Operational_Checklists_ChecklistType DEFAULT (N'');
IF COL_LENGTH('Operational.Checklists', 'Location') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [Location] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Checklists_Location DEFAULT (N'');
IF COL_LENGTH('Operational.Checklists', 'TotalItems') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [TotalItems] int NOT NULL CONSTRAINT DF_Operational_Checklists_TotalItems DEFAULT (0);
IF COL_LENGTH('Operational.Checklists', 'CompletedItems') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [CompletedItems] int NOT NULL CONSTRAINT DF_Operational_Checklists_CompletedItems DEFAULT (0);
IF COL_LENGTH('Operational.Checklists', 'ExceptionCount') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [ExceptionCount] int NOT NULL CONSTRAINT DF_Operational_Checklists_ExceptionCount DEFAULT (0);
IF COL_LENGTH('Operational.Checklists', 'CompletedByUserId') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [CompletedByUserId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.Checklists', 'CompletedAtUtc') IS NULL
    ALTER TABLE [Operational].[Checklists] ADD [CompletedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Operational.ChecklistsHistoryEntry', 'CompletedItemsAtTransition') IS NULL
    ALTER TABLE [Operational].[ChecklistsHistoryEntry] ADD [CompletedItemsAtTransition] int NOT NULL CONSTRAINT DF_Operational_ChecklistsHistoryEntry_CompletedItemsAtTransition DEFAULT (0);
IF COL_LENGTH('Operational.ChecklistsHistoryEntry', 'ExceptionCountAtTransition') IS NULL
    ALTER TABLE [Operational].[ChecklistsHistoryEntry] ADD [ExceptionCountAtTransition] int NOT NULL CONSTRAINT DF_Operational_ChecklistsHistoryEntry_ExceptionCountAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Operational.Contractor / Operational.ContractorHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.Contractor', 'SiteId') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.Contractor', 'ContractorCompany') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [ContractorCompany] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Contractor_ContractorCompany DEFAULT (N'');
IF COL_LENGTH('Operational.Contractor', 'ContractorName') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [ContractorName] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Contractor_ContractorName DEFAULT (N'');
IF COL_LENGTH('Operational.Contractor', 'QualificationStatus') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [QualificationStatus] nvarchar(50) NOT NULL CONSTRAINT DF_Operational_Contractor_QualificationStatus DEFAULT (N'Pending');
IF COL_LENGTH('Operational.Contractor', 'ReadinessScore') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [ReadinessScore] decimal(7,4) NOT NULL CONSTRAINT DF_Operational_Contractor_ReadinessScore DEFAULT (0);
IF COL_LENGTH('Operational.Contractor', 'PrimaryContactId') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [PrimaryContactId] uniqueidentifier NOT NULL CONSTRAINT DF_Operational_Contractor_PrimaryContactId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Operational.Contractor', 'InsuranceExpiresAtUtc') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [InsuranceExpiresAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.Contractor', 'OnboardedAtUtc') IS NULL
    ALTER TABLE [Operational].[Contractor] ADD [OnboardedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Operational.ContractorHistoryEntry', 'ReadinessScoreAtTransition') IS NULL
    ALTER TABLE [Operational].[ContractorHistoryEntry] ADD [ReadinessScoreAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Operational_ContractorHistoryEntry_ReadinessScoreAtTransition DEFAULT (0);
IF COL_LENGTH('Operational.ContractorHistoryEntry', 'QualificationStatusAtTransition') IS NULL
    ALTER TABLE [Operational].[ContractorHistoryEntry] ADD [QualificationStatusAtTransition] nvarchar(50) NULL;
GO

------------------------------------------------------------------------------
-- Operational.PPEManagement / Operational.PPEManagementHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.PPEManagement', 'SiteId') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.PPEManagement', 'WorkerId') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [WorkerId] uniqueidentifier NOT NULL CONSTRAINT DF_Operational_PPEManagement_WorkerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Operational.PPEManagement', 'PpeType') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [PpeType] nvarchar(100) NOT NULL CONSTRAINT DF_Operational_PPEManagement_PpeType DEFAULT (N'');
IF COL_LENGTH('Operational.PPEManagement', 'PpeItem') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [PpeItem] nvarchar(200) NOT NULL CONSTRAINT DF_Operational_PPEManagement_PpeItem DEFAULT (N'');
IF COL_LENGTH('Operational.PPEManagement', 'ComplianceStatus') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [ComplianceStatus] nvarchar(50) NOT NULL CONSTRAINT DF_Operational_PPEManagement_ComplianceStatus DEFAULT (N'Compliant');
IF COL_LENGTH('Operational.PPEManagement', 'IssuedAtUtc') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [IssuedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.PPEManagement', 'ReturnedAtUtc') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [ReturnedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.PPEManagement', 'InspectionDueAtUtc') IS NULL
    ALTER TABLE [Operational].[PPEManagement] ADD [InspectionDueAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Operational.PPEManagementHistoryEntry', 'ComplianceStatusAtTransition') IS NULL
    ALTER TABLE [Operational].[PPEManagementHistoryEntry] ADD [ComplianceStatusAtTransition] nvarchar(50) NULL;
GO

------------------------------------------------------------------------------
-- Operational.RiskManagement / Operational.RiskManagementHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.RiskManagement', 'SiteId') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.RiskManagement', 'HazardDescription') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [HazardDescription] nvarchar(max) NOT NULL CONSTRAINT DF_Operational_RiskManagement_HazardDescription DEFAULT (N'');
IF COL_LENGTH('Operational.RiskManagement', 'RiskCategory') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [RiskCategory] nvarchar(100) NOT NULL CONSTRAINT DF_Operational_RiskManagement_RiskCategory DEFAULT (N'');
IF COL_LENGTH('Operational.RiskManagement', 'LikelihoodRating') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [LikelihoodRating] int NOT NULL CONSTRAINT DF_Operational_RiskManagement_LikelihoodRating DEFAULT (1);
IF COL_LENGTH('Operational.RiskManagement', 'SeverityRating') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [SeverityRating] int NOT NULL CONSTRAINT DF_Operational_RiskManagement_SeverityRating DEFAULT (1);
IF COL_LENGTH('Operational.RiskManagement', 'ControlMeasures') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [ControlMeasures] nvarchar(max) NOT NULL CONSTRAINT DF_Operational_RiskManagement_ControlMeasures DEFAULT (N'');
IF COL_LENGTH('Operational.RiskManagement', 'ResponsiblePartyId') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [ResponsiblePartyId] uniqueidentifier NOT NULL CONSTRAINT DF_Operational_RiskManagement_ResponsiblePartyId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Operational.RiskManagement', 'ReviewDueAtUtc') IS NULL
    ALTER TABLE [Operational].[RiskManagement] ADD [ReviewDueAtUtc] datetime2(7) NULL;
GO
-- NOTE: RiskManagement.RiskScore is a get-only COMPUTED C# property
-- (LikelihoodRating * SeverityRating) with no backing field/setter -- not
-- added as a stored column here (see script header). LikelihoodRating and
-- SeverityRating above are the real stored inputs it derives from.

IF COL_LENGTH('Operational.RiskManagementHistoryEntry', 'RiskScoreAtTransition') IS NULL
    ALTER TABLE [Operational].[RiskManagementHistoryEntry] ADD [RiskScoreAtTransition] int NOT NULL CONSTRAINT DF_Operational_RiskManagementHistoryEntry_RiskScoreAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Operational.Schedules / Operational.SchedulesHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.Schedules', 'SiteId') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.Schedules', 'TaskName') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [TaskName] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Schedules_TaskName DEFAULT (N'');
IF COL_LENGTH('Operational.Schedules', 'RecurrencePattern') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [RecurrencePattern] nvarchar(50) NOT NULL CONSTRAINT DF_Operational_Schedules_RecurrencePattern DEFAULT (N'OneTime');
IF COL_LENGTH('Operational.Schedules', 'AssignedToUserId') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [AssignedToUserId] uniqueidentifier NOT NULL CONSTRAINT DF_Operational_Schedules_AssignedToUserId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Operational.Schedules', 'DueAtUtc') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [DueAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.Schedules', 'SlaWindowMinutes') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [SlaWindowMinutes] int NOT NULL CONSTRAINT DF_Operational_Schedules_SlaWindowMinutes DEFAULT (1440);
IF COL_LENGTH('Operational.Schedules', 'CompletedAtUtc') IS NULL
    ALTER TABLE [Operational].[Schedules] ADD [CompletedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Operational.SchedulesHistoryEntry', 'DueAtUtcAtTransition') IS NULL
    ALTER TABLE [Operational].[SchedulesHistoryEntry] ADD [DueAtUtcAtTransition] datetime2(7) NULL;
GO

------------------------------------------------------------------------------
-- Operational.Training / Operational.TrainingHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Operational.Training', 'SiteId') IS NULL
    ALTER TABLE [Operational].[Training] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Operational.Training', 'CourseName') IS NULL
    ALTER TABLE [Operational].[Training] ADD [CourseName] nvarchar(300) NOT NULL CONSTRAINT DF_Operational_Training_CourseName DEFAULT (N'');
IF COL_LENGTH('Operational.Training', 'CourseCode') IS NULL
    ALTER TABLE [Operational].[Training] ADD [CourseCode] nvarchar(100) NOT NULL CONSTRAINT DF_Operational_Training_CourseCode DEFAULT (N'');
IF COL_LENGTH('Operational.Training', 'WorkerId') IS NULL
    ALTER TABLE [Operational].[Training] ADD [WorkerId] uniqueidentifier NOT NULL CONSTRAINT DF_Operational_Training_WorkerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Operational.Training', 'ComplianceCategory') IS NULL
    ALTER TABLE [Operational].[Training] ADD [ComplianceCategory] nvarchar(100) NOT NULL CONSTRAINT DF_Operational_Training_ComplianceCategory DEFAULT (N'Mandatory');
IF COL_LENGTH('Operational.Training', 'AssignedAtUtc') IS NULL
    ALTER TABLE [Operational].[Training] ADD [AssignedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.Training', 'DueAtUtc') IS NULL
    ALTER TABLE [Operational].[Training] ADD [DueAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.Training', 'CompletedAtUtc') IS NULL
    ALTER TABLE [Operational].[Training] ADD [CompletedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('Operational.Training', 'Score') IS NULL
    ALTER TABLE [Operational].[Training] ADD [Score] decimal(7,4) NULL;
GO

IF COL_LENGTH('Operational.TrainingHistoryEntry', 'ScoreAtTransition') IS NULL
    ALTER TABLE [Operational].[TrainingHistoryEntry] ADD [ScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------------------------
-- SafetyEvents.EmergencyManagement / SafetyEvents.EmergencyManagementHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'SiteId') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'EmergencyType') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [EmergencyType] nvarchar(100) NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_EmergencyType DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'Severity') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [Severity] nvarchar(50) NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_Severity DEFAULT (N'Minor');
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'Location') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [Location] nvarchar(300) NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_Location DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'CasualtyCount') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [CasualtyCount] int NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_CasualtyCount DEFAULT (0);
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'ResponseTeamNotified') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [ResponseTeamNotified] bit NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_ResponseTeamNotified DEFAULT (0);
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'DeclaredAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [DeclaredAtUtc] datetime2(7) NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagement_DeclaredAtUtc DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'StabilizedAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [StabilizedAtUtc] datetime2(7) NULL;
IF COL_LENGTH('SafetyEvents.EmergencyManagement', 'ResolvedAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD [ResolvedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('SafetyEvents.EmergencyManagementHistoryEntry', 'SeverityAtTransition') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagementHistoryEntry] ADD [SeverityAtTransition] nvarchar(50) NULL;
IF COL_LENGTH('SafetyEvents.EmergencyManagementHistoryEntry', 'CasualtyCountAtTransition') IS NULL
    ALTER TABLE [SafetyEvents].[EmergencyManagementHistoryEntry] ADD [CasualtyCountAtTransition] int NOT NULL CONSTRAINT DF_SafetyEvents_EmergencyManagementHistoryEntry_CasualtyCountAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- SafetyEvents.Observation / SafetyEvents.ObservationHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('SafetyEvents.Observation', 'SiteId') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('SafetyEvents.Observation', 'ObserverId') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [ObserverId] uniqueidentifier NOT NULL CONSTRAINT DF_SafetyEvents_Observation_ObserverId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('SafetyEvents.Observation', 'ObservationType') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [ObservationType] nvarchar(50) NOT NULL CONSTRAINT DF_SafetyEvents_Observation_ObservationType DEFAULT (N'SafeBehavior');
IF COL_LENGTH('SafetyEvents.Observation', 'HazardCategory') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [HazardCategory] nvarchar(100) NOT NULL CONSTRAINT DF_SafetyEvents_Observation_HazardCategory DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.Observation', 'Location') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [Location] nvarchar(300) NOT NULL CONSTRAINT DF_SafetyEvents_Observation_Location DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.Observation', 'Description') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [Description] nvarchar(max) NOT NULL CONSTRAINT DF_SafetyEvents_Observation_Description DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.Observation', 'CorrectiveActionRequired') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [CorrectiveActionRequired] bit NOT NULL CONSTRAINT DF_SafetyEvents_Observation_CorrectiveActionRequired DEFAULT (0);
IF COL_LENGTH('SafetyEvents.Observation', 'FollowUpDueAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[Observation] ADD [FollowUpDueAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('SafetyEvents.ObservationHistoryEntry', 'CorrectiveActionRequiredAtTransition') IS NULL
    ALTER TABLE [SafetyEvents].[ObservationHistoryEntry] ADD [CorrectiveActionRequiredAtTransition] bit NOT NULL CONSTRAINT DF_SafetyEvents_ObservationHistoryEntry_CorrectiveActionRequiredAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- SafetyEvents.IncidentManagement / SafetyEvents.IncidentManagementHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'SiteId') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'Title') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [Title] nvarchar(300) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_Title DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'Description') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [Description] nvarchar(max) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_Description DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'Severity') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [Severity] nvarchar(50) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_Severity DEFAULT (N'Low');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'Location') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [Location] nvarchar(300) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_Location DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'IncidentOccurredAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [IncidentOccurredAtUtc] datetime2(7) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_IncidentOccurredAtUtc DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'InjuryType') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [InjuryType] nvarchar(50) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_InjuryType DEFAULT (N'None');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'ReportedByWorkerId') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [ReportedByWorkerId] uniqueidentifier NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_ReportedByWorkerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'AssignedInvestigatorId') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [AssignedInvestigatorId] uniqueidentifier NULL;
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'RootCause') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [RootCause] nvarchar(max) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_RootCause DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'CorrectiveActions') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [CorrectiveActions] nvarchar(max) NOT NULL CONSTRAINT DF_SafetyEvents_IncidentManagement_CorrectiveActions DEFAULT (N'');
IF COL_LENGTH('SafetyEvents.IncidentManagement', 'ClosedAtUtc') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD [ClosedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('SafetyEvents.IncidentManagementHistoryEntry', 'SeverityAtTransition') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagementHistoryEntry] ADD [SeverityAtTransition] nvarchar(50) NULL;
IF COL_LENGTH('SafetyEvents.IncidentManagementHistoryEntry', 'RootCauseAtTransition') IS NULL
    ALTER TABLE [SafetyEvents].[IncidentManagementHistoryEntry] ADD [RootCauseAtTransition] nvarchar(max) NULL;
GO

------------------------------------------------------------------------------
-- Analytics.Culture / Analytics.CultureHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Analytics.Culture', 'SiteId') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.Culture', 'SurveyPeriod') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [SurveyPeriod] nvarchar(20) NOT NULL CONSTRAINT DF_Analytics_Culture_SurveyPeriod DEFAULT (N'');
IF COL_LENGTH('Analytics.Culture', 'CultureIndexScore') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [CultureIndexScore] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_Culture_CultureIndexScore DEFAULT (0);
IF COL_LENGTH('Analytics.Culture', 'EngagementScore') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [EngagementScore] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_Culture_EngagementScore DEFAULT (0);
IF COL_LENGTH('Analytics.Culture', 'ParticipationRate') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [ParticipationRate] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_Culture_ParticipationRate DEFAULT (0);
IF COL_LENGTH('Analytics.Culture', 'TopRiskTheme') IS NULL
    ALTER TABLE [Analytics].[Culture] ADD [TopRiskTheme] nvarchar(300) NOT NULL CONSTRAINT DF_Analytics_Culture_TopRiskTheme DEFAULT (N'');
GO

IF COL_LENGTH('Analytics.CultureHistoryEntry', 'CultureIndexScoreAtTransition') IS NULL
    ALTER TABLE [Analytics].[CultureHistoryEntry] ADD [CultureIndexScoreAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_CultureHistoryEntry_CultureIndexScoreAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Analytics.Dashboard / Analytics.DashboardHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Analytics.Dashboard', 'SiteId') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.Dashboard', 'DashboardName') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [DashboardName] nvarchar(300) NOT NULL CONSTRAINT DF_Analytics_Dashboard_DashboardName DEFAULT (N'');
IF COL_LENGTH('Analytics.Dashboard', 'DashboardType') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [DashboardType] nvarchar(100) NOT NULL CONSTRAINT DF_Analytics_Dashboard_DashboardType DEFAULT (N'');
IF COL_LENGTH('Analytics.Dashboard', 'OwnerId') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [OwnerId] uniqueidentifier NOT NULL CONSTRAINT DF_Analytics_Dashboard_OwnerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Analytics.Dashboard', 'RefreshIntervalSeconds') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [RefreshIntervalSeconds] int NOT NULL CONSTRAINT DF_Analytics_Dashboard_RefreshIntervalSeconds DEFAULT (60);
IF COL_LENGTH('Analytics.Dashboard', 'WidgetCount') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [WidgetCount] int NOT NULL CONSTRAINT DF_Analytics_Dashboard_WidgetCount DEFAULT (0);
IF COL_LENGTH('Analytics.Dashboard', 'IsPublished') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [IsPublished] bit NOT NULL CONSTRAINT DF_Analytics_Dashboard_IsPublished DEFAULT (0);
IF COL_LENGTH('Analytics.Dashboard', 'LastRefreshedAtUtc') IS NULL
    ALTER TABLE [Analytics].[Dashboard] ADD [LastRefreshedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Analytics.DashboardHistoryEntry', 'IsPublishedAtTransition') IS NULL
    ALTER TABLE [Analytics].[DashboardHistoryEntry] ADD [IsPublishedAtTransition] bit NOT NULL CONSTRAINT DF_Analytics_DashboardHistoryEntry_IsPublishedAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Analytics.Engagement / Analytics.EngagementHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Analytics.Engagement', 'SiteId') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.Engagement', 'WorkerId') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [WorkerId] uniqueidentifier NOT NULL CONSTRAINT DF_Analytics_Engagement_WorkerId DEFAULT ('00000000-0000-0000-0000-000000000000');
IF COL_LENGTH('Analytics.Engagement', 'EngagementScore') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [EngagementScore] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_Engagement_EngagementScore DEFAULT (0);
IF COL_LENGTH('Analytics.Engagement', 'SurveyPeriod') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [SurveyPeriod] nvarchar(20) NOT NULL CONSTRAINT DF_Analytics_Engagement_SurveyPeriod DEFAULT (N'');
IF COL_LENGTH('Analytics.Engagement', 'ParticipationChannel') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [ParticipationChannel] nvarchar(100) NOT NULL CONSTRAINT DF_Analytics_Engagement_ParticipationChannel DEFAULT (N'');
IF COL_LENGTH('Analytics.Engagement', 'ResponseCount') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [ResponseCount] int NOT NULL CONSTRAINT DF_Analytics_Engagement_ResponseCount DEFAULT (0);
IF COL_LENGTH('Analytics.Engagement', 'LastEngagedAtUtc') IS NULL
    ALTER TABLE [Analytics].[Engagement] ADD [LastEngagedAtUtc] datetime2(7) NULL;
GO

IF COL_LENGTH('Analytics.EngagementHistoryEntry', 'EngagementScoreAtTransition') IS NULL
    ALTER TABLE [Analytics].[EngagementHistoryEntry] ADD [EngagementScoreAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_EngagementHistoryEntry_EngagementScoreAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Analytics.PredictiveAnalytics / Analytics.PredictiveAnalyticsHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'SiteId') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'ModelName') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [ModelName] nvarchar(200) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_ModelName DEFAULT (N'');
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'PredictionType') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [PredictionType] nvarchar(100) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_PredictionType DEFAULT (N'');
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'TargetEntityId') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [TargetEntityId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'PredictedRiskScore') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [PredictedRiskScore] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_PredictedRiskScore DEFAULT (0);
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'ConfidenceLevel') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [ConfidenceLevel] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_ConfidenceLevel DEFAULT (0);
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'PredictionHorizonDays') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [PredictionHorizonDays] int NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_PredictionHorizonDays DEFAULT (30);
IF COL_LENGTH('Analytics.PredictiveAnalytics', 'GeneratedAtUtc') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD [GeneratedAtUtc] datetime2(7) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalytics_GeneratedAtUtc DEFAULT (SYSUTCDATETIME());
GO

IF COL_LENGTH('Analytics.PredictiveAnalyticsHistoryEntry', 'PredictedRiskScoreAtTransition') IS NULL
    ALTER TABLE [Analytics].[PredictiveAnalyticsHistoryEntry] ADD [PredictedRiskScoreAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_PredictiveAnalyticsHistoryEntry_PredictedRiskScoreAtTransition DEFAULT (0);
GO

------------------------------------------------------------------------------
-- Analytics.ScoreCards / Analytics.ScoreCardsHistoryEntry
------------------------------------------------------------------------------
IF COL_LENGTH('Analytics.ScoreCards', 'SiteId') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [SiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.ScoreCards', 'ScoreCardName') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [ScoreCardName] nvarchar(300) NOT NULL CONSTRAINT DF_Analytics_ScoreCards_ScoreCardName DEFAULT (N'');
IF COL_LENGTH('Analytics.ScoreCards', 'ReportingPeriod') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [ReportingPeriod] nvarchar(20) NOT NULL CONSTRAINT DF_Analytics_ScoreCards_ReportingPeriod DEFAULT (N'');
IF COL_LENGTH('Analytics.ScoreCards', 'KpiCategory') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [KpiCategory] nvarchar(100) NOT NULL CONSTRAINT DF_Analytics_ScoreCards_KpiCategory DEFAULT (N'');
IF COL_LENGTH('Analytics.ScoreCards', 'PerformanceIndex') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [PerformanceIndex] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_ScoreCards_PerformanceIndex DEFAULT (0);
IF COL_LENGTH('Analytics.ScoreCards', 'GeneratedForSiteId') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [GeneratedForSiteId] uniqueidentifier NULL;
IF COL_LENGTH('Analytics.ScoreCards', 'GeneratedAtUtc') IS NULL
    ALTER TABLE [Analytics].[ScoreCards] ADD [GeneratedAtUtc] datetime2(7) NOT NULL CONSTRAINT DF_Analytics_ScoreCards_GeneratedAtUtc DEFAULT (SYSUTCDATETIME());
GO

IF COL_LENGTH('Analytics.ScoreCardsHistoryEntry', 'PerformanceIndexAtTransition') IS NULL
    ALTER TABLE [Analytics].[ScoreCardsHistoryEntry] ADD [PerformanceIndexAtTransition] decimal(7,4) NOT NULL CONSTRAINT DF_Analytics_ScoreCardsHistoryEntry_PerformanceIndexAtTransition DEFAULT (0);
GO
