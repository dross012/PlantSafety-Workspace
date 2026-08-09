-- ============================================================================
--  Project:        AI.HazardUnderstanding -- Phase1-HazardUnderstandingLog
--  File:           01-table-hazardunderstandinglog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIHazardUnderstanding].[HazardUnderstandingLog].
--                  Input scalar (HazardTypeCode -- HazardUnderstandingInput.
--                  HazardType) flattened as a column; the composed
--                  HazardSemanticModel output (Category/RelatedHazards/
--                  Severity/PropagationChain/MultiHazardChain/
--                  RegulatoryImpact -- HazardSemanticModel.cs) serialized as
--                  ModelJson, per this task's "serialize nested rule
--                  outputs as JSON" instruction, since HazardSemanticModel
--                  is itself a composition of several already-normalized
--                  Path B/C structures (HazardChain, RegulatoryJurisdiction
--                  list, etc.) that this engine only aggregates, not owns.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardUnderstandingLog' AND schema_id = SCHEMA_ID('AIHazardUnderstanding'))
BEGIN
    CREATE TABLE [AIHazardUnderstanding].[HazardUnderstandingLog]
    (
        [HazardUnderstandingLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_HazardUnderstandingLog_Id DEFAULT NEWSEQUENTIALID(),
        [InputHazardTypeCode]        NVARCHAR(30)      NOT NULL,
        [InputEmployeeInjuryCount]    INT               NOT NULL,
        [InputMachineFailureCount]    INT               NOT NULL,
        [InputEventChainLength]       INT               NOT NULL,
        [ModelJson]                   NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]              DATETIME2(3)      NOT NULL,

        [CreatedUtc]                  DATETIME2(3)      NOT NULL CONSTRAINT DF_HazardUnderstandingLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                   NVARCHAR(128)     NOT NULL CONSTRAINT DF_HazardUnderstandingLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                   BIT               NOT NULL CONSTRAINT DF_HazardUnderstandingLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_HazardUnderstandingLog PRIMARY KEY CLUSTERED ([HazardUnderstandingLogId]),
        CONSTRAINT FK_HazardUnderstandingLog_HazardType FOREIGN KEY ([InputHazardTypeCode]) REFERENCES [Lookup].[HazardType] ([Code]),
        CONSTRAINT CK_HazardUnderstandingLog_ModelJson CHECK (ISJSON([ModelJson]) = 1)
    );
END
GO
