-- 0096_create_feed_entries.sql
-- Creates the feed_entries table for the My Feed section on the home page.

CREATE TABLE IF NOT EXISTS feed_entries (
    feed_entry_id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(user_id),
    message TEXT NOT NULL,
    feed_type SMALLINT NOT NULL DEFAULT 0,
    group_id BIGINT,
    poster_user_id BIGINT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_feed_entries_user_id ON feed_entries(user_id);
CREATE INDEX IF NOT EXISTS idx_feed_entries_created_at ON feed_entries(created_at DESC);
