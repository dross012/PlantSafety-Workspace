-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.SlaMonitoring
--  Layer:          Infrastructure – SQL Scripts
--  File:           IncidentSlaPolicy.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing ISlaPolicyRepository / SlaPolicyDapperRepository (UI Gaps
--                  Scoping, IncidentManagement Part 2 -- genuinely new SLA policy engine, no
--                  analog existed anywhere in the platform prior to this pass). Seeded with a
--                  sensible platform-default policy set per tenant on first use, since no
--                  admin-authoring UI exists yet for this domain; mirrors
--                  PlantSafety.Domain.SafetyEvents.SlaMonitoring.ValueObjects.SlaPolicy.Default.
--                  A tenant with no rows falls back entirely to Default at the Application layer
--                  (SlaPolicyResolver), so seeding is a performance/audit convenience, not a
--                  correctness requirement. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentSlaPolicy' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[IncidentSlaPolicy]
    (
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        Severity                 NVARCHAR(30)     NOT NULL,
        AcknowledgeWithinMinutes INT              NOT NULL,
        ResolveWithinMinutes     INT              NOT NULL,
        CreatedAtUtc             DATETIME2(3)     NOT NULL CONSTRAINT DF_IncidentSlaPolicy_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IncidentSlaPolicy PRIMARY KEY (TenantId, Severity)
    );
END
GO
