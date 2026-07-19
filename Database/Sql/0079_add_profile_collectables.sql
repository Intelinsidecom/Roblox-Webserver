-- 0079_add_profile_collectables.sql
-- Adds the profile_collectables integer array for user-curated profile collection items

ALTER TABLE users ADD COLUMN IF NOT EXISTS profile_collectables integer[] DEFAULT '{}';
