-- 0022_add_asset_sales.sql
-- Add a sales counter to assets so we can track how many times an item was purchased.

alter table if exists assets
    add column if not exists sales bigint not null default 0;
