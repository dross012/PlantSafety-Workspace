-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Parts.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Part (maintenance parts inventory) aggregate root.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Parts' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[Parts] (
        [PartId]            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [PartNumber]        NVARCHAR(50)     NOT NULL,
        [PartName]          NVARCHAR(200)    NOT NULL,
        [QuantityOnHand]    INT              NOT NULL,
        [ReorderThreshold]  INT              NOT NULL,
        [Location]          NVARCHAR(200)    NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]      DATETIME2(3)     NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT DF_Parts_IsDeleted DEFAULT (0),
        [RowVersion]        ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Parts_TenantId] ON [Operational].[Parts] ([TenantId]);
END
GO
