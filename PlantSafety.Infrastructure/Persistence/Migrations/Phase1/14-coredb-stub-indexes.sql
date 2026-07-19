USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 1
-- Script:        14-coredb-stub-indexes.sql
-- Purpose:       Index Orchestrator pass over the 22 tables (+22 HistoryEntry
--                companions) fleshed out with real business columns by
--                10-coredb-flesh-out-stubs-v2.sql on 2026-07-18, across
--                Compliance (8), Operational (6), SafetyEvents (3), and
--                Analytics (5) schemas in PlantSafety.CoreDB.
--
-- Grounding:     Column lists were read live via sys.columns (not guessed).
--                Existing index/FK surface was read live via sys.indexes and
--                sys.foreign_keys before any index below was authored.
--                Naming/template conventions grounded in
--                sql-database-skill/references/00-canonical-templates.md
--                (`IX_<Entity>_<Column>`), extended with the live
--                IX_<Schema>_<Table>_<Cols> convention already established
--                across all 22 target tables' pre-existing Phase 1 indexes.
--
-- What was found already in place (Phase 1 scaffold, unchanged/not duplicated
-- here):
--   - PK_<Schema>_<Table> clustered PK on every one of the 44 tables.
--   - IX_<Schema>_<Table>_Status_CreatedUtc on every base table:
--     (Status, CreatedUtc DESC) WHERE IsDeleted = 0.
--   - IX_<Schema>_<Table>_TenantId on every base table: (TenantId), no filter.
--   - Every *HistoryEntry table already has IX_<Schema>_<Table>_<Base>Id
--     (FK-supporting) and IX_<Schema>_<Table>_TenantId. These fully cover the
--     only sys.foreign_keys-declared FKs found for this batch (all 22 are
--     HistoryEntry -> base-table parent FKs; see rule 1 note below).
--
-- Rule 1 (FK indexes) verification: sys.foreign_keys was queried for the
-- Compliance/Operational/SafetyEvents/Analytics schemas. The only declared FK
-- constraints are the 22 pre-existing <Table>HistoryEntry -> <Table> parent
-- links, and each already has a supporting index from Phase 1 (see above).
-- None of the new business columns added in 10-coredb-flesh-out-stubs-v2.sql
-- (AuditorId, HolderWorkerId, ResponsiblePartyId, WorkerId, OwnerId,
-- AssignedToUserId, ObserverId, ReportedByWorkerId, etc.) carry a declared FK
-- constraint (they are cross-aggregate/cross-module identity references, not
-- enforced relational FKs) -- per rule 1, "if none exist, skip this" applies
-- to those columns specifically. They ARE, however, addressed below under
-- rule 4 (natural-lookup indexes) on their own merits.
--
-- Rule 2 (recency): the pre-existing IX_..._Status_CreatedUtc index leads
-- with Status, not TenantId, so it does not efficiently serve a
-- tenant-scoped "most recent N records" scan. This script adds
-- IX_<Schema>_<Table>_TenantId_CreatedUtc: (TenantId, CreatedUtc DESC)
-- WHERE IsDeleted = 0 on every base table, matching the
-- EnvironmentalHealth/WorkerWellBeing (TenantId, <recency-col> DESC)
-- filtered pattern. Leading-key-column set (TenantId, CreatedUtc) is
-- distinct from both pre-existing indexes -- not a duplicate.
--
-- Rule 3 (status covering): the pre-existing IX_..._Status_CreatedUtc index
-- leads with Status but is not tenant-scoped, so a single-tenant
-- "my open items by status" query still has to cross tenant boundaries in
-- the seek. This script adds IX_<Schema>_<Table>_TenantId_Status:
-- (TenantId, Status) WHERE IsDeleted = 0 on every base table. Leading-key-
-- column set (TenantId, Status) is distinct from both pre-existing indexes.
--
-- Rule 4 (natural lookups): judgment-based, one per genuinely useful
-- "my items" identity column and one per genuinely useful SLA/expiry
-- datetime column actually present on each table (see per-table comments
-- below). Tables with no such column (EmergencyManagement, Culture) get
-- none -- not over-indexed. All lookups are
-- IX_<Schema>_<Table>_TenantId_<Column>: (TenantId, <Column>)
-- WHERE IsDeleted = 0 [AND <Column> IS NOT NULL for nullable columns].
--
-- HistoryEntry tables: no additional indexes added. Each already has its
-- FK-supporting <Base>Id index and a TenantId index from Phase 1; history
-- rows are append-only and queried almost exclusively by <Base>Id, which is
-- already indexed. Adding recency/status indexes there would not serve any
-- known read path and would violate the "don't over-index" guidance.
--
-- Idempotent:    Every CREATE INDEX is guarded by a sys.indexes existence
--                check (IF NOT EXISTS pattern), safe to re-run.
-- Deletion:      No indexes are dropped by this script (Deletion Discipline
--                rule -- this is an additive-only pass; no candidate-removal
--                list was produced because no redundant index was found
--                among the ones this script creates).
-- =============================================

------------------------------------------------------------------------------
-- Compliance.Auditing
-- Lookup: AuditorId ("my assigned audits")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Auditing_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Auditing'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Auditing_TenantId_CreatedUtc] ON [Compliance].[Auditing] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Auditing_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Auditing'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Auditing_TenantId_Status] ON [Compliance].[Auditing] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Auditing_TenantId_AuditorId' AND object_id = OBJECT_ID('Compliance.Auditing'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Auditing_TenantId_AuditorId] ON [Compliance].[Auditing] ([TenantId], [AuditorId]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Compliance.Certification
-- Lookups: HolderWorkerId ("my certifications"), ExpiresAtUtc (expiry SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Certification_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Certification'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Certification_TenantId_CreatedUtc] ON [Compliance].[Certification] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Certification_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Certification'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Certification_TenantId_Status] ON [Compliance].[Certification] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Certification_TenantId_HolderWorkerId' AND object_id = OBJECT_ID('Compliance.Certification'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Certification_TenantId_HolderWorkerId] ON [Compliance].[Certification] ([TenantId], [HolderWorkerId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Certification_TenantId_ExpiresAtUtc' AND object_id = OBJECT_ID('Compliance.Certification'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Certification_TenantId_ExpiresAtUtc] ON [Compliance].[Certification] ([TenantId], [ExpiresAtUtc]) WHERE [IsDeleted] = 0 AND [ExpiresAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Compliance
-- Lookup: ResponsiblePartyId ("my compliance items")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Compliance_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Compliance'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Compliance_TenantId_CreatedUtc] ON [Compliance].[Compliance] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Compliance_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Compliance'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Compliance_TenantId_Status] ON [Compliance].[Compliance] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Compliance_TenantId_ResponsiblePartyId' AND object_id = OBJECT_ID('Compliance.Compliance'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Compliance_TenantId_ResponsiblePartyId] ON [Compliance].[Compliance] ([TenantId], [ResponsiblePartyId]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Compliance.ManagementOfChange
-- Lookups: RequestedByUserId ("my MOC requests"), ReviewDueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_ManagementOfChange_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.ManagementOfChange'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_ManagementOfChange_TenantId_CreatedUtc] ON [Compliance].[ManagementOfChange] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_ManagementOfChange_TenantId_Status' AND object_id = OBJECT_ID('Compliance.ManagementOfChange'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_ManagementOfChange_TenantId_Status] ON [Compliance].[ManagementOfChange] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_ManagementOfChange_TenantId_RequestedByUserId' AND object_id = OBJECT_ID('Compliance.ManagementOfChange'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_ManagementOfChange_TenantId_RequestedByUserId] ON [Compliance].[ManagementOfChange] ([TenantId], [RequestedByUserId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_ManagementOfChange_TenantId_ReviewDueAtUtc' AND object_id = OBJECT_ID('Compliance.ManagementOfChange'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_ManagementOfChange_TenantId_ReviewDueAtUtc] ON [Compliance].[ManagementOfChange] ([TenantId], [ReviewDueAtUtc]) WHERE [IsDeleted] = 0 AND [ReviewDueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Permits
-- Lookups: RequestedByUserId ("my permit requests"), ExpiresAtUtc (expiry SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Permits_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Permits'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Permits_TenantId_CreatedUtc] ON [Compliance].[Permits] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Permits_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Permits'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Permits_TenantId_Status] ON [Compliance].[Permits] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Permits_TenantId_RequestedByUserId' AND object_id = OBJECT_ID('Compliance.Permits'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Permits_TenantId_RequestedByUserId] ON [Compliance].[Permits] ([TenantId], [RequestedByUserId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Permits_TenantId_ExpiresAtUtc' AND object_id = OBJECT_ID('Compliance.Permits'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Permits_TenantId_ExpiresAtUtc] ON [Compliance].[Permits] ([TenantId], [ExpiresAtUtc]) WHERE [IsDeleted] = 0 AND [ExpiresAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Compliance.Policy
-- Lookup: OwnerId ("my owned policies")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Policy_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Policy'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Policy_TenantId_CreatedUtc] ON [Compliance].[Policy] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Policy_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Policy'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Policy_TenantId_Status] ON [Compliance].[Policy] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Policy_TenantId_OwnerId' AND object_id = OBJECT_ID('Compliance.Policy'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Policy_TenantId_OwnerId] ON [Compliance].[Policy] ([TenantId], [OwnerId]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Compliance.Procedures
-- Lookups: OwnerId ("my owned procedures"), ReviewDueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Procedures_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.Procedures'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Procedures_TenantId_CreatedUtc] ON [Compliance].[Procedures] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Procedures_TenantId_Status' AND object_id = OBJECT_ID('Compliance.Procedures'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Procedures_TenantId_Status] ON [Compliance].[Procedures] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Procedures_TenantId_OwnerId' AND object_id = OBJECT_ID('Compliance.Procedures'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Procedures_TenantId_OwnerId] ON [Compliance].[Procedures] ([TenantId], [OwnerId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_Procedures_TenantId_ReviewDueAtUtc' AND object_id = OBJECT_ID('Compliance.Procedures'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_Procedures_TenantId_ReviewDueAtUtc] ON [Compliance].[Procedures] ([TenantId], [ReviewDueAtUtc]) WHERE [IsDeleted] = 0 AND [ReviewDueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Compliance.RegulatoryCalendar
-- Lookups: ResponsiblePartyId ("my regulatory items"), DueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_RegulatoryCalendar_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Compliance.RegulatoryCalendar'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_RegulatoryCalendar_TenantId_CreatedUtc] ON [Compliance].[RegulatoryCalendar] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_RegulatoryCalendar_TenantId_Status' AND object_id = OBJECT_ID('Compliance.RegulatoryCalendar'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_RegulatoryCalendar_TenantId_Status] ON [Compliance].[RegulatoryCalendar] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_RegulatoryCalendar_TenantId_ResponsiblePartyId' AND object_id = OBJECT_ID('Compliance.RegulatoryCalendar'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_RegulatoryCalendar_TenantId_ResponsiblePartyId] ON [Compliance].[RegulatoryCalendar] ([TenantId], [ResponsiblePartyId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Compliance_RegulatoryCalendar_TenantId_DueAtUtc' AND object_id = OBJECT_ID('Compliance.RegulatoryCalendar'))
    CREATE NONCLUSTERED INDEX [IX_Compliance_RegulatoryCalendar_TenantId_DueAtUtc] ON [Compliance].[RegulatoryCalendar] ([TenantId], [DueAtUtc]) WHERE [IsDeleted] = 0 AND [DueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.Checklists
-- Lookup: CompletedByUserId ("my completed checklists")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Checklists_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.Checklists'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Checklists_TenantId_CreatedUtc] ON [Operational].[Checklists] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Checklists_TenantId_Status' AND object_id = OBJECT_ID('Operational.Checklists'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Checklists_TenantId_Status] ON [Operational].[Checklists] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Checklists_TenantId_CompletedByUserId' AND object_id = OBJECT_ID('Operational.Checklists'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Checklists_TenantId_CompletedByUserId] ON [Operational].[Checklists] ([TenantId], [CompletedByUserId]) WHERE [IsDeleted] = 0 AND [CompletedByUserId] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.Contractor
-- Lookups: PrimaryContactId ("my contractors"), InsuranceExpiresAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Contractor_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.Contractor'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Contractor_TenantId_CreatedUtc] ON [Operational].[Contractor] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Contractor_TenantId_Status' AND object_id = OBJECT_ID('Operational.Contractor'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Contractor_TenantId_Status] ON [Operational].[Contractor] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Contractor_TenantId_PrimaryContactId' AND object_id = OBJECT_ID('Operational.Contractor'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Contractor_TenantId_PrimaryContactId] ON [Operational].[Contractor] ([TenantId], [PrimaryContactId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Contractor_TenantId_InsuranceExpiresAtUtc' AND object_id = OBJECT_ID('Operational.Contractor'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Contractor_TenantId_InsuranceExpiresAtUtc] ON [Operational].[Contractor] ([TenantId], [InsuranceExpiresAtUtc]) WHERE [IsDeleted] = 0 AND [InsuranceExpiresAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.PPEManagement
-- Lookups: WorkerId ("my PPE"), InspectionDueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_PPEManagement_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.PPEManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_PPEManagement_TenantId_CreatedUtc] ON [Operational].[PPEManagement] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_PPEManagement_TenantId_Status' AND object_id = OBJECT_ID('Operational.PPEManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_PPEManagement_TenantId_Status] ON [Operational].[PPEManagement] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_PPEManagement_TenantId_WorkerId' AND object_id = OBJECT_ID('Operational.PPEManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_PPEManagement_TenantId_WorkerId] ON [Operational].[PPEManagement] ([TenantId], [WorkerId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_PPEManagement_TenantId_InspectionDueAtUtc' AND object_id = OBJECT_ID('Operational.PPEManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_PPEManagement_TenantId_InspectionDueAtUtc] ON [Operational].[PPEManagement] ([TenantId], [InspectionDueAtUtc]) WHERE [IsDeleted] = 0 AND [InspectionDueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.RiskManagement
-- Lookups: ResponsiblePartyId ("my risk items"), ReviewDueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_RiskManagement_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.RiskManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_RiskManagement_TenantId_CreatedUtc] ON [Operational].[RiskManagement] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_RiskManagement_TenantId_Status' AND object_id = OBJECT_ID('Operational.RiskManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_RiskManagement_TenantId_Status] ON [Operational].[RiskManagement] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_RiskManagement_TenantId_ResponsiblePartyId' AND object_id = OBJECT_ID('Operational.RiskManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_RiskManagement_TenantId_ResponsiblePartyId] ON [Operational].[RiskManagement] ([TenantId], [ResponsiblePartyId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_RiskManagement_TenantId_ReviewDueAtUtc' AND object_id = OBJECT_ID('Operational.RiskManagement'))
    CREATE NONCLUSTERED INDEX [IX_Operational_RiskManagement_TenantId_ReviewDueAtUtc] ON [Operational].[RiskManagement] ([TenantId], [ReviewDueAtUtc]) WHERE [IsDeleted] = 0 AND [ReviewDueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.Schedules
-- Lookups: AssignedToUserId ("my scheduled tasks"), DueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Schedules_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.Schedules'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Schedules_TenantId_CreatedUtc] ON [Operational].[Schedules] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Schedules_TenantId_Status' AND object_id = OBJECT_ID('Operational.Schedules'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Schedules_TenantId_Status] ON [Operational].[Schedules] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Schedules_TenantId_AssignedToUserId' AND object_id = OBJECT_ID('Operational.Schedules'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Schedules_TenantId_AssignedToUserId] ON [Operational].[Schedules] ([TenantId], [AssignedToUserId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Schedules_TenantId_DueAtUtc' AND object_id = OBJECT_ID('Operational.Schedules'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Schedules_TenantId_DueAtUtc] ON [Operational].[Schedules] ([TenantId], [DueAtUtc]) WHERE [IsDeleted] = 0 AND [DueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Operational.Training
-- Lookups: WorkerId ("my training"), DueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Training_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Operational.Training'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Training_TenantId_CreatedUtc] ON [Operational].[Training] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Training_TenantId_Status' AND object_id = OBJECT_ID('Operational.Training'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Training_TenantId_Status] ON [Operational].[Training] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Training_TenantId_WorkerId' AND object_id = OBJECT_ID('Operational.Training'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Training_TenantId_WorkerId] ON [Operational].[Training] ([TenantId], [WorkerId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Operational_Training_TenantId_DueAtUtc' AND object_id = OBJECT_ID('Operational.Training'))
    CREATE NONCLUSTERED INDEX [IX_Operational_Training_TenantId_DueAtUtc] ON [Operational].[Training] ([TenantId], [DueAtUtc]) WHERE [IsDeleted] = 0 AND [DueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- SafetyEvents.EmergencyManagement
-- No natural-lookup identity/SLA column present (EmergencyType/Severity/
-- Location/CasualtyCount/ResponseTeamNotified/DeclaredAtUtc/StabilizedAtUtc/
-- ResolvedAtUtc -- no worker/owner/assignee id, no forward-looking due date).
-- Recency + status only; not over-indexed.
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_EmergencyManagement_TenantId_CreatedUtc' AND object_id = OBJECT_ID('SafetyEvents.EmergencyManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_EmergencyManagement_TenantId_CreatedUtc] ON [SafetyEvents].[EmergencyManagement] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_EmergencyManagement_TenantId_Status' AND object_id = OBJECT_ID('SafetyEvents.EmergencyManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_EmergencyManagement_TenantId_Status] ON [SafetyEvents].[EmergencyManagement] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- SafetyEvents.IncidentManagement
-- Lookups: ReportedByWorkerId ("my reported incidents"), AssignedInvestigatorId
-- ("my assigned investigations")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_IncidentManagement_TenantId_CreatedUtc' AND object_id = OBJECT_ID('SafetyEvents.IncidentManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_IncidentManagement_TenantId_CreatedUtc] ON [SafetyEvents].[IncidentManagement] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_IncidentManagement_TenantId_Status' AND object_id = OBJECT_ID('SafetyEvents.IncidentManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_IncidentManagement_TenantId_Status] ON [SafetyEvents].[IncidentManagement] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_IncidentManagement_TenantId_ReportedByWorkerId' AND object_id = OBJECT_ID('SafetyEvents.IncidentManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_IncidentManagement_TenantId_ReportedByWorkerId] ON [SafetyEvents].[IncidentManagement] ([TenantId], [ReportedByWorkerId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_IncidentManagement_TenantId_AssignedInvestigatorId' AND object_id = OBJECT_ID('SafetyEvents.IncidentManagement'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_IncidentManagement_TenantId_AssignedInvestigatorId] ON [SafetyEvents].[IncidentManagement] ([TenantId], [AssignedInvestigatorId]) WHERE [IsDeleted] = 0 AND [AssignedInvestigatorId] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- SafetyEvents.Observation
-- Lookups: ObserverId ("my submitted observations"), FollowUpDueAtUtc (SLA)
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_Observation_TenantId_CreatedUtc' AND object_id = OBJECT_ID('SafetyEvents.Observation'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_Observation_TenantId_CreatedUtc] ON [SafetyEvents].[Observation] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_Observation_TenantId_Status' AND object_id = OBJECT_ID('SafetyEvents.Observation'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_Observation_TenantId_Status] ON [SafetyEvents].[Observation] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_Observation_TenantId_ObserverId' AND object_id = OBJECT_ID('SafetyEvents.Observation'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_Observation_TenantId_ObserverId] ON [SafetyEvents].[Observation] ([TenantId], [ObserverId]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyEvents_Observation_TenantId_FollowUpDueAtUtc' AND object_id = OBJECT_ID('SafetyEvents.Observation'))
    CREATE NONCLUSTERED INDEX [IX_SafetyEvents_Observation_TenantId_FollowUpDueAtUtc] ON [SafetyEvents].[Observation] ([TenantId], [FollowUpDueAtUtc]) WHERE [IsDeleted] = 0 AND [FollowUpDueAtUtc] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Analytics.Culture
-- No natural-lookup identity/SLA column present (SurveyPeriod/scores/
-- TopRiskTheme are aggregate-level, not per-user). Recency + status only.
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Culture_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Analytics.Culture'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Culture_TenantId_CreatedUtc] ON [Analytics].[Culture] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Culture_TenantId_Status' AND object_id = OBJECT_ID('Analytics.Culture'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Culture_TenantId_Status] ON [Analytics].[Culture] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Analytics.Dashboard
-- Lookup: OwnerId ("my dashboards")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Dashboard_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Analytics.Dashboard'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Dashboard_TenantId_CreatedUtc] ON [Analytics].[Dashboard] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Dashboard_TenantId_Status' AND object_id = OBJECT_ID('Analytics.Dashboard'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Dashboard_TenantId_Status] ON [Analytics].[Dashboard] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Dashboard_TenantId_OwnerId' AND object_id = OBJECT_ID('Analytics.Dashboard'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Dashboard_TenantId_OwnerId] ON [Analytics].[Dashboard] ([TenantId], [OwnerId]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Analytics.Engagement
-- Lookup: WorkerId ("my engagement records")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Engagement_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Analytics.Engagement'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Engagement_TenantId_CreatedUtc] ON [Analytics].[Engagement] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Engagement_TenantId_Status' AND object_id = OBJECT_ID('Analytics.Engagement'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Engagement_TenantId_Status] ON [Analytics].[Engagement] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_Engagement_TenantId_WorkerId' AND object_id = OBJECT_ID('Analytics.Engagement'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_Engagement_TenantId_WorkerId] ON [Analytics].[Engagement] ([TenantId], [WorkerId]) WHERE [IsDeleted] = 0;
GO

------------------------------------------------------------------------------
-- Analytics.PredictiveAnalytics
-- Lookup: TargetEntityId (cross-reference -- "predictions about entity X")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_PredictiveAnalytics_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Analytics.PredictiveAnalytics'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_PredictiveAnalytics_TenantId_CreatedUtc] ON [Analytics].[PredictiveAnalytics] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_PredictiveAnalytics_TenantId_Status' AND object_id = OBJECT_ID('Analytics.PredictiveAnalytics'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_PredictiveAnalytics_TenantId_Status] ON [Analytics].[PredictiveAnalytics] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_PredictiveAnalytics_TenantId_TargetEntityId' AND object_id = OBJECT_ID('Analytics.PredictiveAnalytics'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_PredictiveAnalytics_TenantId_TargetEntityId] ON [Analytics].[PredictiveAnalytics] ([TenantId], [TargetEntityId]) WHERE [IsDeleted] = 0 AND [TargetEntityId] IS NOT NULL;
GO

------------------------------------------------------------------------------
-- Analytics.ScoreCards
-- Lookup: GeneratedForSiteId (cross-reference -- "scorecards for site X")
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_ScoreCards_TenantId_CreatedUtc' AND object_id = OBJECT_ID('Analytics.ScoreCards'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_ScoreCards_TenantId_CreatedUtc] ON [Analytics].[ScoreCards] ([TenantId], [CreatedUtc] DESC) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_ScoreCards_TenantId_Status' AND object_id = OBJECT_ID('Analytics.ScoreCards'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_ScoreCards_TenantId_Status] ON [Analytics].[ScoreCards] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Analytics_ScoreCards_TenantId_GeneratedForSiteId' AND object_id = OBJECT_ID('Analytics.ScoreCards'))
    CREATE NONCLUSTERED INDEX [IX_Analytics_ScoreCards_TenantId_GeneratedForSiteId] ON [Analytics].[ScoreCards] ([TenantId], [GeneratedForSiteId]) WHERE [IsDeleted] = 0 AND [GeneratedForSiteId] IS NOT NULL;
GO
