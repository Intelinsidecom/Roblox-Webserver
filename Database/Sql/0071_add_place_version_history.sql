-- 0071_add_place_version_history.sql
-- Adds a jsonb column to store version history for place assets.
-- Each entry: { "version": 1, "date": "7/26/2025 9:00:06 AM", "file_hash": "abc..." }

alter table if exists assets
    add column if not exists place_version_history jsonb not null default '[]'::jsonb;
