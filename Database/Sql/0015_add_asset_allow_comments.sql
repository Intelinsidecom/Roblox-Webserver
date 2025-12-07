-- 0015_add_asset_allow_comments.sql
-- Adds allow_comments flag to assets.

alter table if exists assets
    add column if not exists allow_comments boolean not null default false;
