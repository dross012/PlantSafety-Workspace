-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_GetByAuthorizationStatus.sql | Created: 2026-08-04
--  Description: Returns ControlChangeEvent rows matching the given AuthorizationStatus, most
--                recent first. Backs the Control-Change Audit Feed dashboard and
--                ListUnauthorizedControlChangesQuery.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetByAuthorizationStatus]
    @TenantId            UNIQUEIDENTIFIER,
    @AuthorizationStatus NVARCHAR(20)
AS
SET NOCOUNT ON;
BEGIN
    SELECT ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
           ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[ControlChangeEvent]
    WHERE TenantId = @TenantId AND AuthorizationStatus = @AuthorizationStatus
    ORDER BY DetectedAtUtc DESC;
END
GO
