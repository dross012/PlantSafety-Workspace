-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_CyberPhysicalIncident_GetAll.sql | Created: 2026-08-04
--  Description: Returns all CyberPhysicalIncident rows for a tenant (in-memory specification
--                filtering applied by CyberPhysicalIncidentDapperRepository.ListAsync).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetAll]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
           OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[CyberPhysicalIncident]
    WHERE TenantId = @TenantId
    ORDER BY OpenedAtUtc DESC;
END
GO
