-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
--  Layer:          Infrastructure - SQL Scripts
--  File:           DischargePoint.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DischargePoint aggregate root backing IDischargePointRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargePoint' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[DischargePoint] (
        [DischargePointId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
        [PointCode] NVARCHAR(50) NOT NULL,
        [PointName] NVARCHAR(200) NOT NULL,
        [ReceivingWaterBody] NVARCHAR(200) NOT NULL,
        [Outfall] NVARCHAR(50) NOT NULL,
        [Latitude] DECIMAL(9,6) NULL,
        [Longitude] DECIMAL(9,6) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargePoint_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DischargePoint_TenantId_SiteId]
        ON [EnvironmentalHealth].[DischargePoint] ([TenantId], [SiteId])
        INCLUDE ([Status], [PointCode]);
    CREATE NONCLUSTERED INDEX [IX_DischargePoint_DischargePermitId]
        ON [EnvironmentalHealth].[DischargePoint] ([DischargePermitId]);
END
GO