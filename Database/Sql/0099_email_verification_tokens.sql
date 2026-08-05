-- 0099_email_verification_tokens.sql
-- confirm a user owns the email address they registered.


create table if not exists email_verification_tokens
(
    token        text      not null primary key,
    user_id      bigint    not null references users(user_id) on delete cascade,
    email        text      not null,
    created_at   timestamptz not null default now(),
    expires_at   timestamptz not null default (now() + interval '24 hours'),

    constraint uk_email_verification_tokens_user_email unique (user_id, email)
);

create index if not exists idx_email_verification_tokens_user
    on email_verification_tokens (user_id);

create index if not exists idx_email_verification_tokens_expires_at
    on email_verification_tokens (expires_at);
