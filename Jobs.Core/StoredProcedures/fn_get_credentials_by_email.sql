CREATE OR REPLACE FUNCTION fn_get_credentials_by_email_drop(
    in email varchar(100)
) 
RETURNS SETOF "UserPwdStore" AS $$
BEGIN
  RETURN QUERY SELECT "Id", "Email", "Pwd" as "Password" FROM "UserPwdStore" t WHERE "t"."Email" = email;
END
$$ LANGUAGE plpgsql;

# SELECT "Email", "Pwd" as "Password"  FROM fn_get_credentials_by_email('jup@gmail.com');

CREATE OR REPLACE FUNCTION fn_get_credentials_by_email(
    in email varchar(100)
) 
RETURNS TABLE ("Email" VARCHAR, "Password" VARCHAR) 
AS 
$$
BEGIN
  RETURN QUERY SELECT "t"."Email", "t"."Pwd" as "Password" FROM "UserPwdStore" t WHERE "t"."Email" = email;
END
$$ LANGUAGE plpgsql;

# SELECT "Email", "Password" FROM fn_get_credentials_by_email('jup@gmail.com');

