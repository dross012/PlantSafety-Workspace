-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase3-Propagation
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [Propagation].[HazardPropagationEvent]
--                  (Phase C.6, one row per real HazardPropagationEvent --
--                  FacilityId/FromStage/ToStage/OccurredAtUtc) and
--                  [Propagation].[HazardChain] + [Propagation].
--                  [HazardChainNode] (Phase C.9, an ordered, immutable
--                  multi-hazard chain built exclusively via
--                  HazardChainBuilder -- HazardChain owns an ordered list of
--                  HazardChainNode, each carrying Stage/Severity/
--                  OccurredAtUtc/Description).
--
--  PropagationStage (Fire/Smoke/Evacuation/Spill/Vapor/Exposure/
--  MachineFailure/Injury/Shutdown) is its own local vocabulary, distinct
--  from [Lookup].[HazardType] per HazardPropagationEvent.cs's own header
--  ("kept separate so that vocabulary stays a pure hazard classification"),
--  so it is NOT FK'd to [Lookup].[HazardType] -- a CHECK constraint is used
--  instead, since PropagationStage is local to this bounded context, not a
--  cross-context shared enum (unlike SeverityLevel/HazardType/
--  RegulatoryJurisdiction).
--
--  StrongId / PK: HazardPropagationEvent has no natural id in the C# record
--  itself -- a surrogate HazardPropagationEventId (NEWSEQUENTIALID) is used,
--  matching this codebase's precedent for append-only event tables with no
--  caller-supplied id (see AccidentImpactEntry.cs's own approach).
--  HazardChain is keyed by a surrogate HazardChainId; HazardChainNode is
--  keyed by (HazardChainId, SequenceNumber) to preserve the chain's real
--  ordering (Nodes[0]=Origin .. Nodes[^1]=Terminal).
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardPropagationEvent' AND schema_id = SCHEMA_ID('Propagation'))
BEGIN
    CREATE TABLE [Propagation].[HazardPropagationEvent]
    (
        [HazardPropagationEventId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_HazardPropagationEvent_Id DEFAULT NEWSEQUENTIALID(),
        [FacilityId]                 UNIQUEIDENTIFIER  NOT NULL,
        [FromStage]                  NVARCHAR(30)      NOT NULL,
        [ToStage]                    NVARCHAR(30)      NOT NULL,
        [OccurredAtUtc]               DATETIME2(3)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)      NOT NULL CONSTRAINT DF_HazardPropagationEvent_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)     NOT NULL CONSTRAINT DF_HazardPropagationEvent_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                  BIT               NOT NULL CONSTRAINT DF_HazardPropagationEvent_IsDeleted DEFAULT (0),

        CONSTRAINT PK_HazardPropagationEvent PRIMARY KEY CLUSTERED ([HazardPropagationEventId]),
        CONSTRAINT FK_HazardPropagationEvent_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId]),
        CONSTRAINT CK_HazardPropagationEvent_FromStage CHECK ([FromStage] IN
            (N'Fire', N'Smoke', N'Evacuation', N'Spill', N'Vapor', N'Exposure', N'MachineFailure', N'Injury', N'Shutdown')),
        CONSTRAINT CK_HazardPropagationEvent_ToStage CHECK ([ToStage] IN
            (N'Fire', N'Smoke', N'Evacuation', N'Spill', N'Vapor', N'Exposure', N'MachineFailure', N'Injury', N'Shutdown'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardChain' AND schema_id = SCHEMA_ID('Propagation'))
BEGIN
    CREATE TABLE [Propagation].[HazardChain]
    (
        [HazardChainId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_HazardChain_Id DEFAULT NEWSEQUENTIALID(),
        [FacilityId]      UNIQUEIDENTIFIER  NOT NULL,
        [BuiltAtUtc]      DATETIME2(3)      NOT NULL CONSTRAINT DF_HazardChain_BuiltAtUtc DEFAULT SYSUTCDATETIME(),

        [CreatedUtc]      DATETIME2(3)      NOT NULL CONSTRAINT DF_HazardChain_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]       NVARCHAR(128)     NOT NULL CONSTRAINT DF_HazardChain_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]       BIT               NOT NULL CONSTRAINT DF_HazardChain_IsDeleted DEFAULT (0),

        CONSTRAINT PK_HazardChain PRIMARY KEY CLUSTERED ([HazardChainId]),
        CONSTRAINT FK_HazardChain_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardChainNode' AND schema_id = SCHEMA_ID('Propagation'))
BEGIN
    CREATE TABLE [Propagation].[HazardChainNode]
    (
        [HazardChainId]    UNIQUEIDENTIFIER  NOT NULL,
        [SequenceNumber]   INT               NOT NULL, -- 0-based, matches Nodes[i] ordering
        [Stage]             NVARCHAR(30)      NOT NULL,
        [SeverityCode]      NVARCHAR(30)      NOT NULL,
        [OccurredAtUtc]     DATETIME2(3)      NOT NULL,
        [Description]       NVARCHAR(500)     NULL,

        CONSTRAINT PK_HazardChainNode PRIMARY KEY CLUSTERED ([HazardChainId], [SequenceNumber]),
        CONSTRAINT FK_HazardChainNode_HazardChain FOREIGN KEY ([HazardChainId])
            REFERENCES [Propagation].[HazardChain] ([HazardChainId]),
        CONSTRAINT FK_HazardChainNode_SeverityLevel FOREIGN KEY ([SeverityCode])
            REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_HazardChainNode_Stage CHECK ([Stage] IN
            (N'Fire', N'Smoke', N'Evacuation', N'Spill', N'Vapor', N'Exposure', N'MachineFailure', N'Injury', N'Shutdown'))
    );
END
GO
