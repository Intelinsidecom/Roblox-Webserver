-- 0065_add_recent_visits_to_users.sql
-- Adds visited_places and visited_universes arrays to users table

alter table users 
    add column if not exists visited_universes bigint[] not null default '{}'::bigint[];
