-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure - SQL Scripts
--  File:           StandardsEquipmentRequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Equipment-specific ANSI/ASME/ISO consensus-standard requirement rows
--                  (machine guarding, crane/hoist inspection, robot safety, pressure vessel
--                  rules, etc.), optionally tied back to a single StandardsRequirement.
--                  Global regulatory-adjacent reference data mirroring the OSHAStandard/
--                  OSHARequirement precedent, not tenant business data (no TenantId, no
--                  soft-delete). EquipmentType is free text (no DB enum), matching the
--                  established convention of leaving equipment/module classification as
--                  free text on this class of reference table. InspectionIntervalDescription
--                  is free text rather than a single numeric-days column since inspection
--                  intervals vary by standard, equipment type, and jurisdiction (e.g. "Daily
--                  (frequent) / Monthly (periodic) per ASME B30.2", "Every 1-12 months,
--                  qualified person") -- matching the EpaRecordkeepingRequirement-adjacent
--                  DeadlineDescription precedent used elsewhere for the same reason.
--                  ControlSystemCategory is a free-text, nullable ISO 13849 category/PL
--                  string (e.g. "Category 3, PL d"), only meaningful for control-system-
--                  related equipment requirements. ANSI/ASME/ISO are non-government
--                  consensus standards bodies whose standards are frequently incorporated
--                  by reference into OSHA/state regulations, but the standards text itself
--                  is copyrighted -- this table stores requirement metadata/technical
--                  summaries only, not verbatim copyrighted standard text. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StandardsEquipmentRequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[StandardsEquipmentRequirement] (
        [EquipmentRequirementId]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StandardsEquipmentRequirement PRIMARY KEY
                                                                CONSTRAINT DF_StandardsEquipmentRequirement_EquipmentRequirementId DEFAULT (NEWID()),
        [RequirementId]                    UNIQUEIDENTIFIER NULL     CONSTRAINT FK_StandardsEquipmentRequirement_StandardsRequirement REFERENCES [Compliance].[StandardsRequirement] ([RequirementId]),
        [EquipmentType]                    NVARCHAR(50)     NOT NULL,
        [RequirementText]                  NVARCHAR(MAX)    NOT NULL,
        [InspectionIntervalDescription]    NVARCHAR(200)    NULL,
        [ControlSystemCategory]            NVARCHAR(20)     NULL,
        [CreatedOn]                        DATETIME2(3)     NOT NULL CONSTRAINT DF_StandardsEquipmentRequirement_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                         ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_StandardsEquipmentRequirement_RequirementId] ON [Compliance].[StandardsEquipmentRequirement] ([RequirementId]);
    CREATE NONCLUSTERED INDEX [IX_StandardsEquipmentRequirement_EquipmentType] ON [Compliance].[StandardsEquipmentRequirement] ([EquipmentType]);
END
GO

-- Additive-only evolution.
