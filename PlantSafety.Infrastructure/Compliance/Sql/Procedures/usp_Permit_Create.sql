-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Permit_Create.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IPermitRepository.CreateAsync. Creates a new
--                  Permit to Work in Draft status with a generated human-readable PermitNumber.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_Create]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @PermitId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @PermitType        NVARCHAR(100),
    @WorkDescription   NVARCHAR(2000),
    @WorkLocation      NVARCHAR(500),
    @WorkLeadId        UNIQUEIDENTIFIER,
    @ValidFromUtc      DATETIME2(3),
    @ValidToUtc        DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        -- @PermitId is a UUIDv7 (see CorrelationIdGenerator.NewId): its leading hex characters
        -- encode a millisecond timestamp, so two permits created within the same millisecond
        -- (e.g. against a fixed/deterministic test clock) would collide if PermitNumber were
        -- derived from the leading characters. The trailing 12 hex characters (no hyphens) are
        -- always the random tail of a UUIDv7, so they are used instead.
        DECLARE @PermitNumber NVARCHAR(50) = CONCAT(N'PTW-', UPPER(RIGHT(REPLACE(CONVERT(NVARCHAR(36), @PermitId), N'-', N''), 12)));

        INSERT INTO [Compliance].[Permit]
        (
            PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
            ValidFromUtc, ValidToUtc, Status, LastModifiedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PermitId, @TenantId, @SiteId, @PermitType, @PermitNumber, @WorkDescription, @WorkLocation, @WorkLeadId,
            @ValidFromUtc, @ValidToUtc, N'Draft', @NowUtc, @NowUtc, @ActorId, 0
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
