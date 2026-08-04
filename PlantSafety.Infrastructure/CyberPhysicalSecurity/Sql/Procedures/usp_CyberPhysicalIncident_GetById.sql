-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_CyberPhysicalIncident_GetById.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]
    @CyberPhysicalIncidentId UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
           OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[CyberPhysicalIncident]
    WHERE CyberPhysicalIncidentId = @CyberPhysicalIncidentId AND TenantId = @TenantId;
END
GO
