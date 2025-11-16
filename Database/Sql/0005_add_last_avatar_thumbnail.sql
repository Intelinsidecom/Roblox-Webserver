-- 0005_add_last_avatar_thumbnail.sql
-- Adds columns to cache the last rendered avatar thumbnail and the avatar state hash.

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'users'
          AND column_name  = 'last_avatar_thumbnail'
    ) THEN
        ALTER TABLE public.users
            ADD COLUMN last_avatar_thumbnail text;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'users'
          AND column_name  = 'avatar_state_hash'
    ) THEN
        ALTER TABLE public.users
            ADD COLUMN avatar_state_hash text;
    END IF;
END $$;
