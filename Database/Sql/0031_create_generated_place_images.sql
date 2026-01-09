-- Migration: Create generated_place_images table for caching place thumbnails
-- Description: This table caches generated thumbnail hashes by place asset hash to avoid re-rendering
-- Created: 2025-01-09

CREATE TABLE IF NOT EXISTS generated_place_images (
    id BIGSERIAL PRIMARY KEY,
    place_asset_hash VARCHAR(64) NOT NULL UNIQUE,
    generated_icon_hash VARCHAR(64) NOT NULL,
    generated_thumbnail_hash VARCHAR(64) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_generated_place_images_place_asset_hash 
    ON generated_place_images(place_asset_hash);

-- Create index for updated_at to help with cache cleanup
CREATE INDEX IF NOT EXISTS idx_generated_place_images_updated_at 
    ON generated_place_images(updated_at);

-- Add trigger to automatically update updated_at timestamp
CREATE OR REPLACE FUNCTION update_generated_place_images_updated_at()
    RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

DROP TRIGGER IF EXISTS trigger_update_generated_place_images_updated_at ON generated_place_images;
CREATE TRIGGER trigger_update_generated_place_images_updated_at
    BEFORE UPDATE ON generated_place_images
    FOR EACH ROW
    EXECUTE FUNCTION update_generated_place_images_updated_at();

-- Add comments for documentation
COMMENT ON TABLE generated_place_images IS 'Caches generated place thumbnail hashes to avoid re-rendering identical place assets';
COMMENT ON COLUMN generated_place_images.place_asset_hash IS 'Hash of the place asset file content';
COMMENT ON COLUMN generated_place_images.generated_icon_hash IS 'Hash of the generated icon image';
COMMENT ON COLUMN generated_place_images.generated_thumbnail_hash IS 'Hash of the generated thumbnail image';
