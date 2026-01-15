-- 0036_add_place_access_settings.sql
-- Add comprehensive access settings for places (assets with is_place=true)
-- including device compatibility, maximum visitor count, server fill settings, and private server configuration.

alter table if exists assets
    add column if not exists device_compatibility jsonb not null default '[1, 2, 3]'::jsonb;

alter table if exists assets
    add column if not exists max_visitor_count bigint not null default 8;

alter table if exists assets
    add column if not exists server_fill_type integer not null default 0;

alter table if exists assets
    add column if not exists private_servers_allowed boolean not null default true;

alter table if exists assets
    add column if not exists private_servers_free boolean not null default true;

alter table if exists assets
    add column if not exists private_servers_price integer not null default 100;

create index if not exists idx_assets_device_compatibility on assets using gin(device_compatibility) where is_place = true;
create index if not exists idx_assets_max_visitor_count on assets(max_visitor_count) where is_place = true;
create index if not exists idx_assets_server_fill_type on assets(server_fill_type) where is_place = true;
create index if not exists idx_assets_private_servers_allowed on assets(private_servers_allowed) where is_place = true;

