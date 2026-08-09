-- ============================================================================
--  Project:        AI.SafetyCopilot -- Phase1-SafetyCopilotLog
--  File:           01-table-safetycopilotlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AISafetyCopilot].[SafetyCopilotLog] --
--                  Audience (CopilotAudience -- SafetyCopilotInput.Audience)
--                  flattened as a column since it's the real
--                  role-appropriate-guidance discriminator every consumer
--                  filters/reports by; Actions/Recommendations
--                  (SafetyCopilotOutput.cs) serialized as JSON, since both
--                  are pure compositions of the other four Path E engines'
--                  already-logged outputs, per this task's instruction.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyCopilotLog' AND schema_id = SCHEMA_ID('AISafetyCopilot'))
BEGIN
    CREATE TABLE [AISafetyCopilot].[SafetyCopilotLog]
    (
        [SafetyCopilotLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_SafetyCopilotLog_Id DEFAULT NEWSEQUENTIALID(),
        [Audience]             NVARCHAR(20)      NOT NULL,
        [ActionsJson]          NVARCHAR(MAX)     NOT NULL,
        [RecommendationsJson]  NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]       DATETIME2(3)      NOT NULL,

        [CreatedUtc]           DATETIME2(3)      NOT NULL CONSTRAINT DF_SafetyCopilotLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]            NVARCHAR(128)     NOT NULL CONSTRAINT DF_SafetyCopilotLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]            BIT               NOT NULL CONSTRAINT DF_SafetyCopilotLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_SafetyCopilotLog PRIMARY KEY CLUSTERED ([SafetyCopilotLogId]),
        CONSTRAINT CK_SafetyCopilotLog_Audience CHECK ([Audience] IN (N'Employee', N'Supervisor')),
        CONSTRAINT CK_SafetyCopilotLog_ActionsJson CHECK (ISJSON([ActionsJson]) = 1),
        CONSTRAINT CK_SafetyCopilotLog_RecommendationsJson CHECK (ISJSON([RecommendationsJson]) = 1)
    );
END
GO
