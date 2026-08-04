-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AgencyContact_GetByAgencyId.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all AgencyContact rows for a given PublicSafetyAgency, backing
--                  aggregate rehydration. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_AgencyContact_GetByAgencyId]
    @PublicSafetyAgencyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT AgencyContactId, PublicSafetyAgencyId, Role, ContactName, PhoneNumber, Email, LastVerifiedAtUtc, CreatedAtUtc
    FROM [PublicSafetyCommunication].[AgencyContact]
    WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId
    ORDER BY CreatedAtUtc ASC;
END
GO
