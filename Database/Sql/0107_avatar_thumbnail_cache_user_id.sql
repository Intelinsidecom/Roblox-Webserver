-- 0107_avatar_thumbnail_cache_user_id.sql

ALTER TABLE avatar_thumbnail_cache
    ADD COLUMN IF NOT EXISTS user_id bigint;

CREATE INDEX IF NOT EXISTS ix_avatar_thumbnail_cache_user_id
    ON avatar_thumbnail_cache(user_id);
