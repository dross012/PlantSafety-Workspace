-- ============================================================================
--  Module: HazardousWasteManagement.TsdfFacilities | Layer: Infrastructure - SQL Scripts
--  File: usp_TsdfFacilities_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[TsdfFacilities]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @TsdfFacilityId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @FacilityName NVARCHAR(400),
    @EpaFacilityId NVARCHAR(50),
    @PermitExpiresAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[TsdfFacilities]
            ([TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@TsdfFacilityId, @TenantId, @SiteId, @Status, @FacilityName, @EpaFacilityId, @PermitExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
