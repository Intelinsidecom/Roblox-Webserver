-- 0097_create_featured_games.sql
-- Creates the featured_games table for admin-configured featured games on the home page.

CREATE TABLE IF NOT EXISTS featured_games (
    featured_game_id SERIAL PRIMARY KEY,
    universe_id BIGINT NOT NULL REFERENCES universes(universe_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_featured_games_universe_id ON featured_games(universe_id);
