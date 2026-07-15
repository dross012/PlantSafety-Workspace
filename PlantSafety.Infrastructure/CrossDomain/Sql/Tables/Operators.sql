-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Operators
--  Layer:          Infrastructure - SQL Scripts
--  File:           Operators.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Operators read-model, backing
--                  IOperatorsRepository.GetOperatorsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Operators' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Operators] (
        OperatorId           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        FullName              NVARCHAR(200)    NOT NULL,
        CertificationLevel    NVARCHAR(50)     NOT NULL,
        AssignedMachineId      UNIQUEIDENTIFIER NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        CertifiedAtUtc         DATETIME2(3)     NOT NULL,
        CertifiedBy            UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Operators_TenantId_SiteId ON [CrossDomain].[Operators] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Operators_CertifiedAtUtc ON [CrossDomain].[Operators] (CertifiedAtUtc);
END
GO
