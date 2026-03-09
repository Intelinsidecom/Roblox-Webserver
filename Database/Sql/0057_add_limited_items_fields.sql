-- 0057_add_limited_items_fields.sql
-- Add limited items functionality to assets table
-- Supports limited unique items with quantity tracking and expiration dates

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'limited_unique') THEN
        ALTER TABLE assets ADD COLUMN limited_unique BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'limited_quantity') THEN
        ALTER TABLE assets ADD COLUMN limited_quantity BIGINT;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'limited_remaining') THEN
        ALTER TABLE assets ADD COLUMN limited_remaining BIGINT;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'limited_until') THEN
        ALTER TABLE assets ADD COLUMN limited_until TIMESTAMPTZ;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_assets_limited_unique ON assets(limited_unique) WHERE limited_unique = TRUE;
CREATE INDEX IF NOT EXISTS idx_assets_limited_remaining ON assets(limited_remaining) WHERE limited_remaining > 0;
CREATE INDEX IF NOT EXISTS idx_assets_limited_until ON assets(limited_until) WHERE limited_until IS NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.check_constraints 
                   WHERE constraint_name = 'chk_assets_limited_unique_logic') THEN
        ALTER TABLE assets 
        ADD CONSTRAINT chk_assets_limited_unique_logic 
        CHECK (
            (limited_unique = FALSE AND limited_quantity IS NULL AND limited_remaining IS NULL AND limited_until IS NULL) OR
            (limited_unique = TRUE AND limited_quantity IS NOT NULL AND limited_quantity > 0 AND 
             limited_remaining IS NOT NULL AND limited_remaining >= 0 AND limited_remaining <= limited_quantity)
        );
    END IF;
END $$;
