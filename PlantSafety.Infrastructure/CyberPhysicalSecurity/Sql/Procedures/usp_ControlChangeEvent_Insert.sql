-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_Insert.sql | Created: 2026-08-04
--  Description: Appends a new, immutable ControlChangeEvent row. Tamper-evident: RecordHash and
--                PriorRecordHash are never modified after insert (see ControlChangeEvent.sql
--                header). Backs IControlChangeEventRepository.AddAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]
    @ControlChangeEventId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @SiteId               UNIQUEIDENTIFIER,
    @OtAssetId            UNIQUEIDENTIFIER,
    @ChangeDescription    NVARCHAR(2000),
    @ChangedParameter     NVARCHAR(256),
    @OldValue             NVARCHAR(512),
    @NewValue             NVARCHAR(512),
    @ChangeSourceUserId   UNIQUEIDENTIFIER = NULL,
    @AuthorizationStatus  NVARCHAR(20),
    @DetectedAtUtc        DATETIME2(3),
    @PriorRecordHash      CHAR(64) = NULL,
    @RecordHash           CHAR(64),
    @CreatedAtUtc         DATETIME2(3),
    @CreatedBy            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[ControlChangeEvent]
        (ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
         ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy)
        VALUES
        (@ControlChangeEventId, @TenantId, @SiteId, @OtAssetId, @ChangeDescription, @ChangedParameter, @OldValue, @NewValue,
         @ChangeSourceUserId, @AuthorizationStatus, @DetectedAtUtc, @PriorRecordHash, @RecordHash, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
