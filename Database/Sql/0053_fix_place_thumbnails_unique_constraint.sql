-- 0053_fix_place_thumbnails_unique_constraint.sql
-- Fix unique constraint to allow multiple image thumbnails but only one video per place
-- Description: Replace the current unique constraint with one that only restricts video thumbnails
-- Created: 2025-02-15

ALTER TABLE place_thumbnails DROP CONSTRAINT IF EXISTS uk_place_thumbnails_video_type;

DROP INDEX IF EXISTS uk_place_thumbnails_video_only;


CREATE UNIQUE INDEX uk_place_thumbnails_video_only 
ON place_thumbnails (place_id) 
WHERE (thumbnail_type = 'video');
