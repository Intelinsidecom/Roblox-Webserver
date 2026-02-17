-- 0054_add_asset_votes.sql
-- Add upvotes and downvotes columns to assets table for voting system

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'upvotes') THEN
        ALTER TABLE assets ADD COLUMN upvotes bigint NOT NULL DEFAULT 0;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'assets' AND column_name = 'downvotes') THEN
        ALTER TABLE assets ADD COLUMN downvotes bigint NOT NULL DEFAULT 0;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_assets_upvotes ON assets(upvotes);
CREATE INDEX IF NOT EXISTS idx_assets_downvotes ON assets(downvotes);
CREATE INDEX IF NOT EXISTS idx_assets_votes_composite ON assets(upvotes DESC, downvotes DESC);
CREATE INDEX IF NOT EXISTS idx_assets_has_votes ON assets(upvotes) WHERE upvotes > 0 OR downvotes > 0;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.check_constraints 
                   WHERE constraint_name = 'chk_assets_votes_non_negative') THEN
        ALTER TABLE assets 
        ADD CONSTRAINT chk_assets_votes_non_negative 
        CHECK (upvotes >= 0 AND downvotes >= 0);
    END IF;
END $$;
