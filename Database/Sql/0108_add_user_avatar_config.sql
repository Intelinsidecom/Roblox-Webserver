-- 0108_add_user_avatar_config.sql

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'users'
          AND column_name  = 'avatar_config'
    ) THEN
        ALTER TABLE public.users ADD COLUMN avatar_config jsonb NOT NULL DEFAULT '{}'::jsonb;
    END IF;
END $$;

UPDATE public.users
SET avatar_config = jsonb_build_object(
        'height', 1.0::numeric,
        'width', 1.0::numeric,
        'head', 1.0::numeric,
        'depth', 1.0::numeric,
        'proportion', 0.0::numeric,
        'bodyType', 0.0::numeric,
        'playerAvatarType', 'R15'
    ) || avatar_config
WHERE avatar_config IS NULL
   OR avatar_config <> (
        jsonb_build_object(
            'height', 1.0::numeric,
            'width', 1.0::numeric,
            'head', 1.0::numeric,
            'depth', 1.0::numeric,
            'proportion', 0.0::numeric,
            'bodyType', 0.0::numeric,
            'playerAvatarType', 'R15'
        ) || avatar_config
    );

ALTER TABLE public.users DROP CONSTRAINT IF EXISTS chk_users_avatar_config_object;
ALTER TABLE public.users ADD CONSTRAINT chk_users_avatar_config_object CHECK (jsonb_typeof(avatar_config) = 'object');
ALTER TABLE public.users DROP CONSTRAINT IF EXISTS chk_users_avatar_config_avatar_type;
ALTER TABLE public.users ADD CONSTRAINT chk_users_avatar_config_avatar_type
CHECK (avatar_config ->> 'playerAvatarType' IS NULL OR (avatar_config ->> 'playerAvatarType') IN ('R6', 'R15'));
INSERT INTO public.schemaversions (scriptname, applied)
SELECT '0108_add_user_avatar_config.sql', now()
WHERE NOT EXISTS (
    SELECT 1 FROM public.schemaversions WHERE scriptname = '0108_add_user_avatar_config.sql'
);
