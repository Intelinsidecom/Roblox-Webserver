-- 0072_add_universe_aliases.sql
-- Add aliases JSONB column to universes table for storing asset aliases
-- (e.g., named references to images/decals within a universe)

ALTER TABLE IF EXISTS universes
    ADD COLUMN IF NOT EXISTS aliases jsonb NOT NULL DEFAULT '[]'::jsonb;

CREATE INDEX IF NOT EXISTS idx_universes_aliases
    ON universes USING gin (aliases);
