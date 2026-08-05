-- 0034_fix_place_thumbnails_constraints.sql
-- Fix place_thumbnails constraints to allow multiple image thumbnails but limit video thumbnails
-- Description: Removes the restrictive unique constraint and adds proper constraints
-- Created: 2025-01-10
--
-- Fully idempotent: every prior name this script has ever used
-- (uk_place_thumbnails_place_type, uk_place_thumbnails_video_only, and any
-- underlying auto-named index from a UNIQUE constraint of the same name) is
-- dropped before the final partial unique index is created.

-- 1. Drop the original restrictive unique constraint
ALTER TABLE place_thumbnails DROP CONSTRAINT IF EXISTS uk_place_thumbnails_place_type;

-- 2. Drop any intermediate UNIQUE constraint named uk_place_thumbnails_video_only
--    (a constraint of the same name may have been left behind by an earlier run)
ALTER TABLE place_thumbnails DROP CONSTRAINT IF EXISTS uk_place_thumbnails_video_only;

-- 3. Drop the index form too. CREATE UNIQUE INDEX ON ... UNIQUE (a,b) WHERE ...
--    and ADD CONSTRAINT ... UNIQUE (a,b) both occupy the same namespace, and a
--    previous run may have created either one with this name. Drop both safely.
DROP INDEX IF EXISTS place_thumbnails.uk_place_thumbnails_video_only;

-- 4. Final desired state: partial unique index on (place_id) WHERE thumbnail_type='video'
--    so only one video per place is allowed, but any number of images.
CREATE UNIQUE INDEX IF NOT EXISTS uk_place_thumbnails_one_video_per_place
    ON place_thumbnails (place_id)
    WHERE thumbnail_type = 'video';
