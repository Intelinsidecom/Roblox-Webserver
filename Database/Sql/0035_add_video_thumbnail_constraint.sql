-- 0035_add_video_thumbnail_constraint.sql
-- Add proper unique constraint for video thumbnails to support ON CONFLICT
-- Description: Creates a unique constraint that allows multiple images but only one video per place
-- Created: 2025-01-11

-- Drop any existing constraints/indexes that might conflict
DROP INDEX IF EXISTS uk_place_thumbnails_video_only;
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'uk_place_thumbnails_place_video' 
        AND table_name = 'place_thumbnails'
    ) THEN
        ALTER TABLE place_thumbnails 
            DROP CONSTRAINT uk_place_thumbnails_place_video;
    END IF;
END $$;

-- Create a partial unique index that only applies to video thumbnails
-- This allows multiple images but only one video per place
CREATE UNIQUE INDEX IF NOT EXISTS uk_place_thumbnails_video_only 
    ON place_thumbnails(place_id) 
    WHERE thumbnail_type = 'video';
