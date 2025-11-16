-- 0006_create_avatar_thumbnail_cache.sql
-- Global cache for rendered avatar thumbnails keyed by configuration hash.

CREATE TABLE IF NOT EXISTS avatar_thumbnail_cache (
    config_hash  text PRIMARY KEY,
    image_hash   text NOT NULL,
    file_name    text NOT NULL,
    render_type  text NOT NULL,
    width        integer NOT NULL,
    height       integer NOT NULL,
    created_at   timestamptz NOT NULL DEFAULT now()
);
