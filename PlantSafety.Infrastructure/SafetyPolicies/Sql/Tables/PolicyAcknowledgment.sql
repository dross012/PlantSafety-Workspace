-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PolicyAcknowledgment.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Child table backing the SafetyPolicy.Acknowledgments collection
--                  (PolicyAcknowledgment entity). TenantId is denormalized from the parent
--                  SafetyPolicy for Row-Level-Security discovery/enforcement.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyAcknowledgment' AND schema_id = SCHEMA_ID('SafetyPolicies'))
BEGIN
    CREATE TABLE [SafetyPolicies].[PolicyAcknowledgment]
    (
        AcknowledgmentId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PolicyAcknowledgment PRIMARY KEY,
        SafetyPolicyId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PolicyAcknowledgment_SafetyPolicy REFERENCES [SafetyPolicies].[SafetyPolicy] (SafetyPolicyId),
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        AcknowledgedPolicyVersion INT              NOT NULL,
        UserId                    UNIQUEIDENTIFIER NOT NULL,
        AcknowledgedAtUtc         DATETIME2(3)     NOT NULL,
        IsDeleted                 BIT              NOT NULL CONSTRAINT DF_PolicyAcknowledgment_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_PolicyAcknowledgment_SafetyPolicyId ON [SafetyPolicies].[PolicyAcknowledgment] (SafetyPolicyId, IsDeleted);
END
GO
