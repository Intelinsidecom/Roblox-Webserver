-- 0064_add_visit_count_to_assets.sql
-- Adds visit_count column to assets table for tracking total place visits

alter table universes
    add column if not exists visit_count integer not null default 0;

create index if not exists idx_universes_visit_count on universes (visit_count desc);
