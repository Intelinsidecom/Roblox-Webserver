-- 0090_add_in_studio_column.sql
-- Adds in_studio column to track Roblox Studio presence

ALTER TABLE users ADD COLUMN IF NOT EXISTS in_studio boolean NOT NULL DEFAULT false;
CREATE INDEX IF NOT EXISTS idx_users_in_studio ON users (in_studio) WHERE in_studio = true;
