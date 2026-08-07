-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           NFPA_HazardModuleMapping.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many NFPA-hazard-to-PlantSafety-module mapping (mirrors
--                  Compliance.OSHARequirementModuleMapping / NIOSHHazardModuleMapping).
--                  ModuleName is intentionally free text, not a DB-enforced enum -- same open
--                  vocabulary as OSHARequirement.AppliesToModule/OSHARequirementModuleMapping.
--                  Module -- values are the real PlantSafety.Infrastructure top-level module
--                  folder names (e.g. PermitToWork, MachineSafety, EnvironmentalHealth,
--                  HazardousWasteManagement, PPE, OperationalSafety, EmployeeSafety).
--                  MappingRuleCode/Rationale make every mapping traceable back to the exact
--                  deterministic rule that produced it -- never a black box. Global reference
--                  data, no TenantId. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NFPA_HazardModuleMapping' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NFPA_HazardModuleMapping] (
        [MappingId]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NFPA_HazardModuleMapping_MappingId DEFAULT (NEWID()) CONSTRAINT PK_NFPA_HazardModuleMapping PRIMARY KEY,
        [HazardId]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_NFPA_HazardModuleMapping_NFPA_HazardProfile REFERENCES [Compliance].[NFPA_HazardProfile] ([HazardId]),
        [ModuleName]       NVARCHAR(100)    NOT NULL,
        [MappingRuleCode]  NVARCHAR(100)    NOT NULL,
        [Rationale]        NVARCHAR(MAX)    NOT NULL,
        [Severity]         NVARCHAR(20)     NOT NULL,
        RowVersion         ROWVERSION       NOT NULL,
        CONSTRAINT UX_NFPA_HazardModuleMapping_HazardId_ModuleName UNIQUE (HazardId, ModuleName),
        CONSTRAINT CK_NFPA_HazardModuleMapping_Severity CHECK ([Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical'))
    );

    CREATE NONCLUSTERED INDEX [IX_NFPA_HazardModuleMapping_ModuleName] ON [Compliance].[NFPA_HazardModuleMapping] ([ModuleName]);
    CREATE NONCLUSTERED INDEX [IX_NFPA_HazardModuleMapping_HazardId] ON [Compliance].[NFPA_HazardModuleMapping] ([HazardId]);
END
GO
