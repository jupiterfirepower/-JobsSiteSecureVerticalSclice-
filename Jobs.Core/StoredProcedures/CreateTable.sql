CREATE TABLE if not exists "UserPwdStore"
(
  "Id" serial PRIMARY KEY, 
  "Email" VARCHAR (100) NOT NULL,
  "Pwd" VARCHAR (100) NOT NULL,
  UNIQUE("Email")
);

CREATE TABLE if not exists apikeystore
(
  id serial PRIMARY KEY, 
  key VARCHAR (100) NOT NULL,
  expired  TIMESTAMPTZ NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

# created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP