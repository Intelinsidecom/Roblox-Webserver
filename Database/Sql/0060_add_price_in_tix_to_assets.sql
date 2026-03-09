-- 0060_add_price_in_tix_to_assets.sql
-- Adds price_in_tix column to assets table for Tix pricing support

alter table if exists assets
    add column if not exists price_in_tix bigint not null default 0;

create index if not exists idx_assets_price_in_tix on assets(price_in_tix) where price_in_tix > 0;
