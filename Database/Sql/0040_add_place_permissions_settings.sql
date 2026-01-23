-- 0040_add_place_permissions_settings.sql
-- Add permissions settings for places including gear genre restrictions, 
-- allowed gear types (stored as JSON array), and copying permissions.

alter table if exists assets
    add column if not exists is_all_genres_allowed boolean not null default false;

alter table if exists assets
    add column if not exists allowed_gear_types jsonb not null default '[]'::jsonb;

alter table if exists assets
    add column if not exists is_copying_allowed boolean not null default false;

create index if not exists idx_assets_is_all_genres_allowed on assets(is_all_genres_allowed) where is_place = true;
create index if not exists idx_assets_allowed_gear_types on assets using gin(allowed_gear_types) where is_place = true;
create index if not exists idx_assets_is_copying_allowed on assets(is_copying_allowed) where is_place = true;
