-- 0037_add_custom_social_slots.sql
-- Add custom social slots field for places to store the number of reserved friend slots when server_fill_type is Custom

alter table if exists assets
    add column if not exists number_of_custom_social_slots integer not null default 4;

create index if not exists idx_assets_number_of_custom_social_slots on assets(number_of_custom_social_slots) where is_place = true;
