-- ============================================================
-- Migration 012: Fix field_type constraint to accept uppercase values
-- ============================================================
-- Issue: Backend converts field_type to uppercase but database constraint only accepts lowercase
-- Solution: Update constraint to accept both lowercase and uppercase values

\echo '============================================================'
\echo 'Migration 012: Fixing field_type constraint'
\echo '============================================================'

-- Drop the old constraint
ALTER TABLE custom_field_definitions 
DROP CONSTRAINT IF EXISTS custom_field_definitions_field_type_check;

-- Add new constraint that accepts both uppercase and lowercase
ALTER TABLE custom_field_definitions 
ADD CONSTRAINT custom_field_definitions_field_type_check 
CHECK (field_type IN ('text', 'number', 'date', 'boolean', 'select', 'multiselect', 'TEXT', 'NUMBER', 'DATE', 'BOOLEAN', 'SELECT', 'MULTISELECT'));

\echo 'Constraint updated successfully'
