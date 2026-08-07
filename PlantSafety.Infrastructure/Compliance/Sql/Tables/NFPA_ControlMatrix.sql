-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           NFPA_ControlMatrix.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Control-recommendation rows for a Compliance.NFPA_HazardProfile hazard
--                  (ControlType: PPE/Training/Inspection/Permit/Engineering), each carrying a
--                  Severity (ComplianceSeverity smart-enum code: Advisory/Low/Medium/High/
--                  Critical -- see PlantSafety.Domain.Compliance.RulesEngine.Enums.
--                  ComplianceSeverity) and a Rationale explaining why the control is
--                  recommended. Voluntary consensus-standard guidance, not enforceable
--                  regulatory obligation -- never a black box. Global reference data, same
--                  scoping rules as NFPA_HazardProfile (no TenantId). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NFPA_ControlMatrix' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NFPA_ControlMatrix] (
        [MatrixId]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NFPA_ControlMatrix_MatrixId DEFAULT (NEWID()) CONSTRAINT PK_NFPA_ControlMatrix PRIMARY KEY,
        [HazardId]           UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_NFPA_ControlMatrix_NFPA_HazardProfile REFERENCES [Compliance].[NFPA_HazardProfile] ([HazardId]),
        [ControlType]        NVARCHAR(50)     NOT NULL,
        [ControlDescription] NVARCHAR(MAX)    NOT NULL,
        [Severity]           NVARCHAR(20)     NOT NULL,
        [Rationale]          NVARCHAR(MAX)    NOT NULL,
        RowVersion           ROWVERSION       NOT NULL,
        CONSTRAINT CK_NFPA_ControlMatrix_Severity CHECK ([Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical'))
    );

    CREATE NONCLUSTERED INDEX [IX_NFPA_ControlMatrix_HazardId] ON [Compliance].[NFPA_ControlMatrix] ([HazardId]);
    CREATE NONCLUSTERED INDEX [IX_NFPA_ControlMatrix_ControlType] ON [Compliance].[NFPA_ControlMatrix] ([ControlType]);
END
GO
