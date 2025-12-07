-- 0013_add_asset_high_res_thumbnail_url.sql
-- Adds a high_res_thumbnail_url column to assets for higher resolution thumbnails.

alter table if exists assets
    add column if not exists high_res_thumbnail_url text;
