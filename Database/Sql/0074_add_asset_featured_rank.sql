-- 0074_add_asset_featured_rank.sql
-- Adds a featured_rank column so admins can pin top items (1-4) on the catalog front page.

alter table if exists assets
    add column if not exists featured_rank integer not null default 0;
