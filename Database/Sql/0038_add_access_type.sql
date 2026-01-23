-- 0038_add_access_type.sql
-- Add access_type column to assets table to store place access settings
-- 1 = Everyone, 2 = Friends

alter table if exists assets
    add column if not exists access_type integer not null default 1;

create index if not exists idx_assets_access_type on assets(access_type) where is_place = true;
