-- 0032_create_place_thumbnails.sql
-- Create place_thumbnails table for managing multiple thumbnails per place
-- Description: Stores thumbnail metadata including images and videos for places
-- Created: 2025-01-10

CREATE TABLE IF NOT EXISTS place_thumbnails (
    id BIGSERIAL PRIMARY KEY,
    place_id BIGINT NOT NULL,
    
    -- Thumbnail metadata
    thumbnail_type VARCHAR(10) NOT NULL CHECK (thumbnail_type IN ('image', 'video')),
    url TEXT NOT NULL,
    alt_text TEXT NULL,
    file_hash VARCHAR(64) NULL,
    
    -- Video-specific fields
    video_url TEXT NULL,
    video_hash VARCHAR(64) NULL,
    
    -- Sorting and management
    sort_order INTEGER NOT NULL DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Foreign key constraint
    CONSTRAINT fk_place_thumbnails_place_id 
        FOREIGN KEY (place_id) REFERENCES assets(asset_id) ON DELETE CASCADE
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_place_thumbnails_place_id 
    ON place_thumbnails(place_id);

CREATE INDEX IF NOT EXISTS idx_place_thumbnails_type 
    ON place_thumbnails(thumbnail_type);

-- Only one video thumbnail per place (multiple images are fine).
-- Use a partial unique index so only rows with thumbnail_type='video' participate,
-- which matches the intent described in the comment above and is idempotent.
CREATE UNIQUE INDEX IF NOT EXISTS uk_place_thumbnails_one_video_per_place
    ON place_thumbnails (place_id)
    WHERE thumbnail_type = 'video';

CREATE INDEX IF NOT EXISTS idx_place_thumbnails_sort_order 
    ON place_thumbnails(place_id, sort_order);

-- Create index for file hash lookups
CREATE INDEX IF NOT EXISTS idx_place_thumbnails_file_hash 
    ON place_thumbnails(file_hash) WHERE file_hash IS NOT NULL;

-- Add trigger to automatically update updated_at timestamp
CREATE OR REPLACE FUNCTION update_place_thumbnails_updated_at()
    RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

DROP TRIGGER IF EXISTS trigger_update_place_thumbnails_updated_at ON place_thumbnails;
CREATE TRIGGER trigger_update_place_thumbnails_updated_at
    BEFORE UPDATE ON place_thumbnails
    FOR EACH ROW
    EXECUTE FUNCTION update_place_thumbnails_updated_at();

