-- Migration: Add dimension support to generated_place_images table
-- Description: Adds width and height columns to support dimension-specific caching
-- Created: 2025-01-10

-- Add width and height columns to support dimension-specific caching
ALTER TABLE generated_place_images 
ADD COLUMN IF NOT EXISTS width INTEGER,
ADD COLUMN IF NOT EXISTS height INTEGER;

-- Drop the old unique constraint on place_asset_hash only
ALTER TABLE generated_place_images 
DROP CONSTRAINT IF EXISTS generated_place_images_place_asset_hash_key;

-- Create a new composite unique constraint including dimensions
ALTER TABLE generated_place_images 
ADD CONSTRAINT generated_place_images_place_asset_hash_dimensions_unique 
UNIQUE (place_asset_hash, width, height);

-- Update existing records to have default dimensions (assuming square icons)
-- We'll use NULL for width/height to represent the original cached entries
-- This allows us to distinguish between old entries and new dimension-specific ones
UPDATE generated_place_images 
SET width = NULL, height = NULL 
WHERE width IS NULL AND height IS NULL;

-- Create indexes for performance with the new dimensions
CREATE INDEX IF NOT EXISTS idx_generated_place_images_place_asset_hash_dimensions 
    ON generated_place_images(place_asset_hash, width, height);

-- Add comments for documentation
COMMENT ON COLUMN generated_place_images.width IS 'Width of the generated thumbnail (NULL for legacy entries)';
COMMENT ON COLUMN generated_place_images.height IS 'Height of the generated thumbnail (NULL for legacy entries)';
