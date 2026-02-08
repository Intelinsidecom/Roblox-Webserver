-- 0040_add_root_place_id.sql
-- Add root_place_id column to universes table for dedicated start place management
-- This migration moves away from using place_ids[0] as the start place

ALTER TABLE universes 
ADD COLUMN IF NOT EXISTS root_place_id bigint;

CREATE INDEX IF NOT EXISTS idx_universes_root_place_id ON universes(root_place_id);

-- Migrate existing data: set root_place_id to the first place_id in the array
UPDATE universes 
SET root_place_id = place_ids[1] 
WHERE place_ids IS NOT NULL 
  AND array_length(place_ids, 1) > 0 
  AND root_place_id IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'fk_universes_root_place_id'
    ) THEN
        ALTER TABLE universes 
        ADD CONSTRAINT fk_universes_root_place_id 
        FOREIGN KEY (root_place_id) REFERENCES assets(asset_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;
    END IF;
END $$;
