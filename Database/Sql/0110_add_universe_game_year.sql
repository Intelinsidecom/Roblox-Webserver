-- 0110_add_universe_game_year.sql

alter table universes add column if not exists game_year text not null default '2016';
