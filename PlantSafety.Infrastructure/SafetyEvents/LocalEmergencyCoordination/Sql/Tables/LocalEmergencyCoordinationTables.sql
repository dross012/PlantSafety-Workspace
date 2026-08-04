-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           LocalEmergencyCoordinationTables.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LocalEmergencyCoordination aggregate root
--                  ([SafetyEvents].[LocalEmergencyCoordinationCases]) and its four child entity
--                  tables (MutualAidAgreements, JointResponsePlans, ExternalResponderLiaisons,
--                  LiaisonLogEntries, JointDebriefRecords). Mirrors EmergencyManagementItems.sql's
--                  idempotent CREATE-if-not-exists pattern. Liaison logs are retained with the
--                  incident record for the full incident retention period per the module's data
--                  governance requirement (retention policy enforced by the platform's shared
--                  retention job, not by this DDL).
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LocalEmergencyCoordinationCases' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[LocalEmergencyCoordinationCases] (
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        SiteId                       UNIQUEIDENTIFIER NULL,
        IncidentId                   UNIQUEIDENTIFIER NULL,
        CaseName                     NVARCHAR(200)    NOT NULL,
        Status                       NVARCHAR(50)     NOT NULL,
        CreatedAtUtc                 DATETIME2(3)     NOT NULL,
        CreatedBy                    UNIQUEIDENTIFIER NOT NULL,
        ClosedAtUtc                  DATETIME2(3)     NULL,
        ArchivedAtUtc                DATETIME2(3)     NULL,
        IsDeleted                    BIT              NOT NULL CONSTRAINT DF_LocalEmergencyCoordinationCases_IsDeleted DEFAULT (0),
        RowVersion                   ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LocalEmergencyCoordinationCases_TenantId_SiteId] ON [SafetyEvents].[LocalEmergencyCoordinationCases] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_LocalEmergencyCoordinationCases_IncidentId] ON [SafetyEvents].[LocalEmergencyCoordinationCases] (IncidentId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MutualAidAgreements' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[MutualAidAgreements] (
        MutualAidAgreementId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        AgencyName                   NVARCHAR(200)    NOT NULL,
        Discipline                   NVARCHAR(50)     NOT NULL,
        DocumentVersion              INT              NOT NULL,
        EffectiveFromUtc             DATETIME2(3)     NOT NULL,
        RenewalDueAtUtc              DATETIME2(3)     NOT NULL,
        RenewalStatus                NVARCHAR(30)     NOT NULL,
        EstablishedAtUtc             DATETIME2(3)     NOT NULL,
        EstablishedBy                UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MutualAidAgreements_CaseId] ON [SafetyEvents].[MutualAidAgreements] (LocalEmergencyCoordinationId);
    CREATE NONCLUSTERED INDEX [IX_MutualAidAgreements_TenantId_RenewalDueAtUtc] ON [SafetyEvents].[MutualAidAgreements] (TenantId, RenewalDueAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JointResponsePlans' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[JointResponsePlans] (
        JointResponsePlanId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        IncidentId                   UNIQUEIDENTIFIER NULL,
        PlanName                     NVARCHAR(200)    NOT NULL,
        IsActive                     BIT              NOT NULL,
        ActivatedAtUtc                DATETIME2(3)    NOT NULL,
        ActivatedBy                   UNIQUEIDENTIFIER NOT NULL,
        DeactivatedAtUtc              DATETIME2(3)    NULL,
        DeactivatedBy                 UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_JointResponsePlans_CaseId] ON [SafetyEvents].[JointResponsePlans] (LocalEmergencyCoordinationId);
    CREATE NONCLUSTERED INDEX [IX_JointResponsePlans_IncidentId] ON [SafetyEvents].[JointResponsePlans] (IncidentId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExternalResponderLiaisons' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[ExternalResponderLiaisons] (
        ExternalResponderLiaisonId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        IncidentId                   UNIQUEIDENTIFIER NULL,
        AgencyName                   NVARCHAR(200)    NOT NULL,
        LiaisonUserId                UNIQUEIDENTIFIER NOT NULL,
        AssignedAtUtc                DATETIME2(3)     NOT NULL,
        AssignedBy                   UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ExternalResponderLiaisons_CaseId] ON [SafetyEvents].[ExternalResponderLiaisons] (LocalEmergencyCoordinationId);
    CREATE NONCLUSTERED INDEX [IX_ExternalResponderLiaisons_IncidentId] ON [SafetyEvents].[ExternalResponderLiaisons] (IncidentId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LiaisonLogEntries' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[LiaisonLogEntries] (
        LiaisonLogEntryId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ExternalResponderLiaisonId   UNIQUEIDENTIFIER NOT NULL,
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        IncidentId                   UNIQUEIDENTIFIER NULL,
        AgencyName                   NVARCHAR(200)    NOT NULL,
        Message                      NVARCHAR(2000)   NOT NULL,
        LoggedAtUtc                  DATETIME2(3)     NOT NULL,
        LoggedBy                     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LiaisonLogEntries_LiaisonId] ON [SafetyEvents].[LiaisonLogEntries] (ExternalResponderLiaisonId);
    CREATE NONCLUSTERED INDEX [IX_LiaisonLogEntries_IncidentId] ON [SafetyEvents].[LiaisonLogEntries] (IncidentId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JointDebriefRecords' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[JointDebriefRecords] (
        JointDebriefRecordId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        LocalEmergencyCoordinationId UNIQUEIDENTIFIER NOT NULL,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        IncidentId                   UNIQUEIDENTIFIER NULL,
        AgenciesInvolved             NVARCHAR(500)    NOT NULL,
        Summary                      NVARCHAR(4000)   NOT NULL,
        RecordedAtUtc                DATETIME2(3)     NOT NULL,
        RecordedBy                   UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_JointDebriefRecords_CaseId] ON [SafetyEvents].[JointDebriefRecords] (LocalEmergencyCoordinationId);
    CREATE NONCLUSTERED INDEX [IX_JointDebriefRecords_TenantId_RecordedAtUtc] ON [SafetyEvents].[JointDebriefRecords] (TenantId, RecordedAtUtc);
END
GO
