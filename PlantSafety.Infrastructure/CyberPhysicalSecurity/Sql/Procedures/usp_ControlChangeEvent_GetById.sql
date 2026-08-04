-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_GetById.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetById]
    @ControlChangeEventId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
           ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[ControlChangeEvent]
    WHERE ControlChangeEventId = @ControlChangeEventId AND TenantId = @TenantId;
END
GO
