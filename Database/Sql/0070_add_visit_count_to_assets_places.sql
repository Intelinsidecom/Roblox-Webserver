-- 0070_add_visit_count_to_assets_places.sql
-- Adds visit_count column to assets table for tracking individual place visits

alter table assets
    add column if not exists visit_count integer not null default 0;

create index if not exists idx_assets_visit_count on assets (visit_count desc);
