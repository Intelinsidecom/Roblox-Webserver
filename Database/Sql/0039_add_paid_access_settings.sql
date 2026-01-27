-- 0039_add_paid_access_settings.sql
-- Add paid access settings for places (assets with is_place=true)
-- including paid access enabled flag and price

alter table if exists assets
    add column if not exists paid_access_enabled boolean not null default false;

alter table if exists assets
    add column if not exists paid_access_price integer not null default 100;

create index if not exists idx_assets_paid_access_enabled on assets(paid_access_enabled) where is_place = true;
create index if not exists idx_assets_paid_access_price on assets(paid_access_price) where is_place = true;
