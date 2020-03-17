create table "b_user"
(
    id          text not null
        constraint b_user_pk
            primary key,
    pw          text not null,
    email       text,
    phone       text,
    email_check boolean default false,
    phone_check boolean default false
);

comment on column "b_user".id is '用户ID';

comment on column "b_user".pw is '用户密码';

comment on column "b_user".email is '邮箱';

comment on column "b_user".phone is '电话';

comment on column "b_user".email_check is '验证位 	0:未验证
	1:已验证';

comment on column "b_user".phone_check is '验证位 	0:未验证
	1:已验证';

alter table "b_user"
    owner to "unityServer";