-- 0021_add_avatar_render_urls_json.sql
-- Adds a JSONB avatar_render_urls payload to users for per-user avatar/headshot render URLs.
-- Structure example: {"avatar": "https://...", "headshot": "https://..."}

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'users'
          AND column_name  = 'avatar_render_urls'
    ) THEN
        ALTER TABLE public.users
            ADD COLUMN avatar_render_urls jsonb NOT NULL DEFAULT '{}'::jsonb;
    END IF;

    -- Best-effort migration of existing thumbnail/headshot columns into JSON.
    -- This is safe to re-run because it only overwrites keys when the source
    -- column is non-null and non-empty.
    UPDATE public.users
    SET avatar_render_urls = (
        COALESCE(avatar_render_urls, '{}'::jsonb)
        || CASE WHEN thumbnail_url IS NOT NULL AND thumbnail_url <> ''
                THEN jsonb_build_object('avatar', thumbnail_url)
                ELSE '{}'::jsonb
           END
        || CASE WHEN headshot_url IS NOT NULL AND headshot_url <> ''
                THEN jsonb_build_object('headshot', headshot_url)
                ELSE '{}'::jsonb
           END
    );
END $$;
