-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicSafetyAgency_GetAllActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PublicSafetyAgency rows. Backs
--                  IPublicSafetyAgencyRepository.ListAsync (specification evaluated in-memory).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
        PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PublicSafetyCommunication].[PublicSafetyAgency]
    WHERE IsDeleted = 0;
END
GO
