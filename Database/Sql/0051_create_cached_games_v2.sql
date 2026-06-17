DROP TABLE IF EXISTS cached_games CASCADE;

CREATE TABLE cached_games (
    id              SERIAL PRIMARY KEY,
    universe_id     BIGINT NOT NULL,
    cached_at       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    sort_filter     INTEGER NOT NULL,
    genre_filter    INTEGER NOT NULL DEFAULT 1,
    cache_order     INTEGER NOT NULL,

    CONSTRAINT fk_cached_games_universe FOREIGN KEY (universe_id) REFERENCES universes(universe_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_cached_games_filter_lookup ON cached_games(sort_filter, genre_filter, cached_at);
CREATE INDEX IF NOT EXISTS idx_cached_games_order ON cached_games(sort_filter, genre_filter, cache_order);
CREATE INDEX IF NOT EXISTS idx_cached_games_universe ON cached_games(universe_id);
CREATE INDEX IF NOT EXISTS idx_cached_games_cached_at ON cached_games(cached_at);
