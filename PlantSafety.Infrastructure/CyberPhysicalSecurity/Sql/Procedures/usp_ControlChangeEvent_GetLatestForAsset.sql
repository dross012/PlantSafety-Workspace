-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_GetLatestForAsset.sql | Created: 2026-08-04
--  Description: Returns the most recently recorded ControlChangeEvent for an asset, used to chain
--                PriorRecordHash when recording the next change (tamper-evident hash chain).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetLatestForAsset]
    @OtAssetId UNIQUEIDENTIFIER,
    @TenantId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT TOP (1) ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
           ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[ControlChangeEvent]
    WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId
    ORDER BY DetectedAtUtc DESC, CreatedAtUtc DESC;
END
GO
