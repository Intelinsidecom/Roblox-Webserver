-- Datastore tables migration (PostgreSQL)
-- Replaces the earlier UUID-keyed datastores / datastore_entries schema with a
-- single table keyed on (place_id, scope, target, key).

DROP TABLE IF EXISTS datastore_entries;
DROP TABLE IF EXISTS datastores;

CREATE TABLE datastore_entries (
    id bigserial PRIMARY KEY,
    place_id bigint NOT NULL,
    scope text NOT NULL,
    target text NOT NULL,
    key text NOT NULL,
    value jsonb NOT NULL,
    sort_key double precision,
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_datastore_entries UNIQUE (place_id, scope, target, key)
);

CREATE INDEX IF NOT EXISTS ix_datastore_entries_sorted
    ON datastore_entries (place_id, scope, target, sort_key)
    WHERE sort_key IS NOT NULL;
