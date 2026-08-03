-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           RouteVersionSnapshot.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Table backing the RouteVersionSnapshot aggregate root
--                  (IRouteVersionSnapshotRepository). Immutable, append-only historical capture
--                  of a RouteDefinition's step content at each version bump -- backs the Version
--                  History timeline and the Diff Viewer. Genuinely new; no analog exists for
--                  ProcessDefinition. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ProcessManagement')
    EXEC('CREATE SCHEMA ProcessManagement AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RouteVersionSnapshot' AND schema_id = SCHEMA_ID('ProcessManagement'))
BEGIN
    CREATE TABLE [ProcessManagement].[RouteVersionSnapshot]
    (
        RouteVersionId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RouteVersionSnapshot PRIMARY KEY,
        RouteDefinitionId    UNIQUEIDENTIFIER NOT NULL,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        VersionNumber        INT              NOT NULL,
        Status               NVARCHAR(50)     NOT NULL CONSTRAINT DF_RouteVersionSnapshot_Status DEFAULT (N'Pending'),
        EffectiveDateUtc     DATETIME2(3)     NOT NULL,
        StepsJson            NVARCHAR(MAX)    NOT NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_RouteVersionSnapshot_Route ON [ProcessManagement].[RouteVersionSnapshot] (RouteDefinitionId, TenantId, VersionNumber DESC);
END
GO
