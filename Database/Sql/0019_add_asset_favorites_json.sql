-- 0019_add_asset_favorites_json.sql
-- Adds a JSONB favorites payload to assets for per-user favorites.

alter table if exists assets
    add column if not exists favorites jsonb not null default '[]'::jsonb;
