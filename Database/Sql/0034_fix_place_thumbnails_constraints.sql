-- 0034_fix_place_thumbnails_constraints.sql
-- Fix place_thumbnails constraints to allow multiple image thumbnails but limit video thumbnails
-- Description: Removes the restrictive unique constraint and adds proper constraints
-- Created: 2025-01-10

-- First, drop the existing restrictive unique constraint
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'uk_place_thumbnails_place_type' 
        AND table_name = 'place_thumbnails'
    ) THEN
        ALTER TABLE place_thumbnails 
            DROP CONSTRAINT uk_place_thumbnails_place_type;
    END IF;
END $$;

-- Add a unique constraint that only applies to video thumbnails (allowing multiple images)
-- This ensures only one video per place but allows unlimited images
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'uk_place_thumbnails_video_only' 
        AND table_name = 'place_thumbnails'
    ) THEN
        ALTER TABLE place_thumbnails 
            ADD CONSTRAINT uk_place_thumbnails_video_only 
            UNIQUE (place_id, thumbnail_type) 
            DEFERRABLE INITIALLY DEFERRED;
    END IF;
END $$;

-- Actually, let's create a partial unique index for videos only instead
-- Drop the constraint if it was created
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'uk_place_thumbnails_video_only' 
        AND table_name = 'place_thumbnails'
    ) THEN
        ALTER TABLE place_thumbnails 
            DROP CONSTRAINT uk_place_thumbnails_video_only;
    END IF;
END $$;

-- Create a partial unique index that only applies to video thumbnails
-- This allows multiple images but only one video per place
CREATE UNIQUE INDEX IF NOT EXISTS uk_place_thumbnails_video_only 
    ON place_thumbnails(place_id, thumbnail_type) 
    WHERE thumbnail_type = 'video';
