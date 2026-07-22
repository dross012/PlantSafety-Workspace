-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.HazardCommunicationRightToKnow
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationRightToKnow_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted HazardCommunicationRightToKnow rows, backing
--                  IHazardCommunicationRightToKnowRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_HazardCommunicationRightToKnow_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardCommunicationRightToKnowId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HazardClass], [SdsReference], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [LabeledItemCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[HazardCommunicationRightToKnow]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
