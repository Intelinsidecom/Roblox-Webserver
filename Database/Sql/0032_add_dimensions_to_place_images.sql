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

ALTER TABLE generated_place_images DROP CONSTRAINT IF EXISTS generated_place_images_place_asset_hash_dimensions_unique;
DROP INDEX IF EXISTS generated_place_images.generated_place_images_place_asset_hash_dimensions_unique;

CREATE UNIQUE INDEX IF NOT EXISTS idx_generated_place_images_hash_dims_unique
    ON generated_place_images (place_asset_hash, width, height);

UPDATE generated_place_images
SET width = NULL, height = NULL
WHERE width IS NULL AND height IS NULL;

CREATE INDEX IF NOT EXISTS idx_generated_place_images_place_asset_hash_dimensions
    ON generated_place_images(place_asset_hash, width, height);
