-- ============================================================
-- Migration 013: Fix NULL values in configuration_change_history
-- ============================================================
-- Issue: Some history records have NULL entity_type which causes InvalidCastException
-- Solution: Update NULL values and ensure all required fields are populated

\echo '============================================================'
\echo 'Migration 013: Fixing NULL values in history'
\echo '============================================================'

-- Update records with NULL entity_type to have a default value
UPDATE configuration_change_history 
SET entity_type = 'CONFIGURATION'
WHERE entity_type IS NULL;

-- Update records with NULL from_version/to_version
UPDATE configuration_change_history 
SET from_version = 0
WHERE from_version IS NULL;

UPDATE configuration_change_history 
SET to_version = 0
WHERE to_version IS NULL;

\echo 'History NULL values fixed'
