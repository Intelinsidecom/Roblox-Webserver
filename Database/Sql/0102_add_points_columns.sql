-- 0102_add_points_columns.sql
-- Universal points system:
--   * users.total_points        = the user's balance across all games (sum of all universes)
--   * user_universe_points      = per-universe balance (awarded points in that game)

alter table users add column if not exists total_points bigint not null default 0;

create table if not exists user_universe_points (
    user_id     bigint not null references users(user_id) on delete cascade,
    universe_id bigint not null references universes(universe_id) on delete cascade,
    points      bigint not null default 0,
    primary key (user_id, universe_id)
);

create index if not exists idx_user_universe_points_universe on user_universe_points(universe_id);
