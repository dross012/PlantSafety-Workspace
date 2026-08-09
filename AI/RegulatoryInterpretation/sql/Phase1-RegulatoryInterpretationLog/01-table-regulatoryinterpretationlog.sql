-- ============================================================================
--  Project:        AI.RegulatoryInterpretation -- Phase1-RegulatoryInterpretationLog
--  File:           01-table-regulatoryinterpretationlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIRegulatoryInterpretation].
--                  [RegulatoryInterpretationLog]. Input scalars
--                  (HazardTypeCode/SeverityCode -- RegulatoryInterpretationInput.
--                  HazardType/SeverityLevel) flattened as FK'd columns;
--                  output (RegulatoryInterpretationRule list, one per
--                  applicable jurisdiction) serialized as InterpretationsJson.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryInterpretationLog' AND schema_id = SCHEMA_ID('AIRegulatoryInterpretation'))
BEGIN
    CREATE TABLE [AIRegulatoryInterpretation].[RegulatoryInterpretationLog]
    (
        [RegulatoryInterpretationLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_RegulatoryInterpretationLog_Id DEFAULT NEWSEQUENTIALID(),
        [InputHazardTypeCode]             NVARCHAR(30)      NOT NULL,
        [InputSeverityCode]                NVARCHAR(30)      NOT NULL,
        [InterpretationsJson]              NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]                   DATETIME2(3)      NOT NULL,

        [CreatedUtc]                       DATETIME2(3)      NOT NULL CONSTRAINT DF_RegulatoryInterpretationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                        NVARCHAR(128)     NOT NULL CONSTRAINT DF_RegulatoryInterpretationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                        BIT               NOT NULL CONSTRAINT DF_RegulatoryInterpretationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_RegulatoryInterpretationLog PRIMARY KEY CLUSTERED ([RegulatoryInterpretationLogId]),
        CONSTRAINT FK_RegulatoryInterpretationLog_HazardType FOREIGN KEY ([InputHazardTypeCode]) REFERENCES [Lookup].[HazardType] ([Code]),
        CONSTRAINT FK_RegulatoryInterpretationLog_SeverityLevel FOREIGN KEY ([InputSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_RegulatoryInterpretationLog_InterpretationsJson CHECK (ISJSON([InterpretationsJson]) = 1)
    );
END
GO
