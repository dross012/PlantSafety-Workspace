-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AgencyContact.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the AgencyContact child entity of the PublicSafetyAgency
--                  aggregate root. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AgencyContact' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
BEGIN
    CREATE TABLE [PublicSafetyCommunication].[AgencyContact]
    (
        AgencyContactId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AgencyContact PRIMARY KEY,
        PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AgencyContact_PublicSafetyAgency REFERENCES [PublicSafetyCommunication].[PublicSafetyAgency] (PublicSafetyAgencyId),
        Role NVARCHAR(64) NOT NULL,
        ContactName NVARCHAR(400) NOT NULL,
        PhoneNumber NVARCHAR(64) NOT NULL,
        Email NVARCHAR(320) NULL,
        LastVerifiedAtUtc DATETIME2(3) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AgencyContact_PublicSafetyAgencyId ON [PublicSafetyCommunication].[AgencyContact] (PublicSafetyAgencyId);
END
GO
