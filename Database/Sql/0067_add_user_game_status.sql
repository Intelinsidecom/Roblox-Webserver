-- 0067_add_user_game_status.sql
-- Adds in_game status and current_place_id to users table for tracking player presence

alter table users 
    add column if not exists in_game boolean not null default false;

alter table users 
    add column if not exists current_place_id bigint null;

create index if not exists idx_users_in_game on users (in_game) where in_game = true;
create index if not exists idx_users_current_place on users (current_place_id) where current_place_id is not null;
