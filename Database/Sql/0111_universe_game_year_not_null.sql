-- 0111_universe_game_year_not_null.sql

update universes set game_year = '2016' where game_year is null or game_year = '';

alter table universes alter column game_year set default '2016';
alter table universes alter column game_year set not null;
