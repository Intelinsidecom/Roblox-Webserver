-- 0009_add_assets_last_updated.sql
-- Add last_updated column to assets for friendly "Updated:" display.

alter table if exists assets
    add column if not exists last_updated timestamptz not null default now();

create index if not exists idx_assets_last_updated on assets(last_updated);
