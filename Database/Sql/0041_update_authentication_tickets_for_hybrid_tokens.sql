-- 0041_update_authentication_tickets_for_hybrid_tokens.sql
-- Updates the authentication_tickets table to support the new TokenService

ALTER TABLE authentication_tickets 
ADD COLUMN IF NOT EXISTS memory_cached BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS cache_expires_at TIMESTAMPTZ;

UPDATE authentication_tickets 
SET memory_cached = false 
WHERE memory_cached IS NULL;

CREATE INDEX IF NOT EXISTS idx_authentication_tickets_memory_cached_expires 
ON authentication_tickets (memory_cached, cache_expires_at) 
WHERE memory_cached = true;

CREATE OR REPLACE FUNCTION cleanup_expired_authentication_tickets()
RETURNS void AS $$
BEGIN
    UPDATE authentication_tickets 
    SET is_active = false 
    WHERE is_active = true 
    AND (
        (memory_cached = false AND expires_at < NOW() - INTERVAL '1 hour')
        OR
        (memory_cached = true AND cache_expires_at < NOW())
    );

    DELETE FROM authentication_tickets 
    WHERE expires_at < NOW() - INTERVAL '7 days';
END;
$$ LANGUAGE plpgsql;
