-- 0014_add_asset_sale_fields.sql
-- Adds on_sale and price columns to assets for catalog sales.

alter table if exists assets
    add column if not exists on_sale boolean not null default false,
    add column if not exists price bigint not null default 0;
