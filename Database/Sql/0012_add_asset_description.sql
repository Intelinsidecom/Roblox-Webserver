-- 0012_add_asset_description.sql
-- Adds a description column to assets for storing user-facing descriptions.

alter table if exists assets
    add column if not exists description text;
