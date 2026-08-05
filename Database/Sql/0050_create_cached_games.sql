-- 0050_create_cached_games.sql
-- v1 schema for cached_games. Superseded by 0051_create_cached_games_v2.sql
-- which drops and recreates this table. This file is kept idempotent and
-- self-healing so a partial run from a wiped migration ledger doesn't leave
-- the DB in a state that breaks v2.

-- Drop any partial/broken cached_games left over from earlier attempts so the
-- recreated table has every column the FKs and v2 rely on.
DROP TABLE IF EXISTS cached_games CASCADE;

CREATE TABLE IF NOT EXISTS cached_games (
    id SERIAL PRIMARY KEY,
    universe_id BIGINT NOT NULL,
    place_id BIGINT NOT NULL,
    name VARCHAR(255) NOT NULL,
    creator_name VARCHAR(100) NOT NULL,
    creator_user_id BIGINT NOT NULL,
    icon_url VARCHAR(500) NOT NULL DEFAULT '/images/blocked.png',
    thumbnail_url VARCHAR(500) NOT NULL DEFAULT '/images/blocked.png',
    playing INTEGER NOT NULL DEFAULT 0,
    up_votes INTEGER NOT NULL DEFAULT 0,
    down_votes INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    cached_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    sort_filter INTEGER NOT NULL,
    genre_filter INTEGER NOT NULL DEFAULT 1,
    cache_order INTEGER NOT NULL,

    -- Drop-then-add keeps reruns against a partially-applied schema idempotent.
    CONSTRAINT fk_cached_games_universe FOREIGN KEY (universe_id) REFERENCES universes(universe_id) ON DELETE CASCADE,
    CONSTRAINT fk_cached_games_place FOREIGN KEY (place_id) REFERENCES assets(asset_id) ON DELETE CASCADE,
    CONSTRAINT fk_cached_games_creator FOREIGN KEY (creator_user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Indexes (idempotent).
CREATE INDEX IF NOT EXISTS idx_cached_games_filter_lookup ON cached_games(sort_filter, genre_filter, cached_at);
CREATE INDEX IF NOT EXISTS idx_cached_games_order ON cached_games(sort_filter, genre_filter, cache_order);
CREATE INDEX IF NOT EXISTS idx_cached_games_universe ON cached_games(universe_id);
CREATE INDEX IF NOT EXISTS idx_cached_games_place ON cached_games(place_id);
CREATE INDEX IF NOT EXISTS idx_cached_games_created_at ON cached_games(cached_at);

COMMENT ON TABLE cached_games IS 'Cached game data for improved performance in games listing';
COMMENT ON COLUMN cached_games.sort_filter IS 'Sort filter type (1=Popular, 11=Top Rated, etc.)';
COMMENT ON COLUMN cached_games.genre_filter IS 'Genre filter (1=All, other=specific genre)';
COMMENT ON COLUMN cached_games.cache_order IS 'Order within the cached result set';
COMMENT ON COLUMN cached_games.cached_at IS 'When this cache entry was created';
