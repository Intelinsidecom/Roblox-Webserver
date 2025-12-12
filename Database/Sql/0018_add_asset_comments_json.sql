-- 0018_add_asset_comments_json.sql
-- Adds a JSONB comments payload to assets for catalog item comments.

alter table if exists assets
    add column if not exists comments jsonb not null default '[]'::jsonb;
