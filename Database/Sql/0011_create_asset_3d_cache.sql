CREATE TABLE IF NOT EXISTS asset_3d_cache (
    asset_id     bigint PRIMARY KEY,
    model_hash   text NOT NULL,
    obj_file_name text NOT NULL,
    mtl_file_name text NOT NULL,
    width        integer NOT NULL,
    height       integer NOT NULL,
    created_at   timestamptz NOT NULL DEFAULT now()
);
