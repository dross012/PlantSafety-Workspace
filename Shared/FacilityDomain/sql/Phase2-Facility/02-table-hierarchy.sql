-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase2-Facility
--  File:           02-table-hierarchy.sql
--  Created:        2026-08-09
--  Description:    Table DDL for the Phase C.5 three-level physical
--                  hierarchy (FacilityHierarchy.cs: FacilityBuilding ->
--                  FacilityArea -> FacilityZone), each keyed by its own
--                  StrongId (BuildingId/AreaId/ZoneId). Parent-child FKs
--                  express the containment tree directly rather than a
--                  single JSON blob, since FacilityAggregate.AddBuilding/
--                  AddArea/AddZone each mutate one level in place -- a
--                  normalized model is the faithful mirror.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityBuilding' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityBuilding]
    (
        [BuildingId]    UNIQUEIDENTIFIER  NOT NULL,
        [FacilityId]    UNIQUEIDENTIFIER  NOT NULL,
        [Name]          NVARCHAR(200)     NOT NULL,
        [CreatedUtc]    DATETIME2(3)      NOT NULL CONSTRAINT DF_FacilityBuilding_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [IsDeleted]     BIT               NOT NULL CONSTRAINT DF_FacilityBuilding_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityBuilding PRIMARY KEY CLUSTERED ([BuildingId]),
        CONSTRAINT FK_FacilityBuilding_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityArea' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityArea]
    (
        [AreaId]        UNIQUEIDENTIFIER  NOT NULL,
        [BuildingId]    UNIQUEIDENTIFIER  NOT NULL,
        [Name]          NVARCHAR(200)     NOT NULL,
        [CreatedUtc]    DATETIME2(3)      NOT NULL CONSTRAINT DF_FacilityArea_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [IsDeleted]     BIT               NOT NULL CONSTRAINT DF_FacilityArea_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityArea PRIMARY KEY CLUSTERED ([AreaId]),
        CONSTRAINT FK_FacilityArea_Building FOREIGN KEY ([BuildingId])
            REFERENCES [FacilityDomain].[FacilityBuilding] ([BuildingId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityZone' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityZone]
    (
        [ZoneId]        UNIQUEIDENTIFIER  NOT NULL,
        [AreaId]        UNIQUEIDENTIFIER  NOT NULL,
        [Name]          NVARCHAR(200)     NOT NULL,
        [CreatedUtc]    DATETIME2(3)      NOT NULL CONSTRAINT DF_FacilityZone_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [IsDeleted]     BIT               NOT NULL CONSTRAINT DF_FacilityZone_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityZone PRIMARY KEY CLUSTERED ([ZoneId]),
        CONSTRAINT FK_FacilityZone_Area FOREIGN KEY ([AreaId])
            REFERENCES [FacilityDomain].[FacilityArea] ([AreaId])
    );
END
GO

------------------------------------------------------------
-- FacilityDomain.FacilityLifecycleEvent
-- (Phase C.12: FacilityActivatedEvent/FacilityShutdownEvent/
--  FacilityReopenedEvent/FacilityDecommissionedEvent -- append-only history
--  of every real lifecycle transition, alongside Facility.LifecycleStatus's
--  current-state column above.)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityLifecycleEvent' AND schema_id = SCHEMA_ID('FacilityDomain'))
BEGIN
    CREATE TABLE [FacilityDomain].[FacilityLifecycleEvent]
    (
        [FacilityLifecycleEventId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_FacilityLifecycleEvent_Id DEFAULT NEWSEQUENTIALID(),
        [FacilityId]                 UNIQUEIDENTIFIER  NOT NULL,
        [EventType]                  NVARCHAR(30)      NOT NULL,
        [Reason]                     NVARCHAR(500)     NULL,
        [OccurredAtUtc]               DATETIME2(3)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)      NOT NULL CONSTRAINT DF_FacilityLifecycleEvent_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)     NOT NULL CONSTRAINT DF_FacilityLifecycleEvent_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                  BIT               NOT NULL CONSTRAINT DF_FacilityLifecycleEvent_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FacilityLifecycleEvent PRIMARY KEY CLUSTERED ([FacilityLifecycleEventId]),
        CONSTRAINT FK_FacilityLifecycleEvent_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId]),
        CONSTRAINT CK_FacilityLifecycleEvent_EventType CHECK ([EventType] IN (N'Activated', N'ShutDown', N'Reopened', N'Decommissioned'))
    );
END
GO
