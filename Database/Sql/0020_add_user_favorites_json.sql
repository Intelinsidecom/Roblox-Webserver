-- 0020_add_user_favorites_json.sql
-- Adds a JSONB favorites payload to users for per-user favorite tracking.

alter table if exists users
    add column if not exists favorites jsonb not null default '[]'::jsonb;
