create or replace procedure sp_add_credentials(
    in email varchar(100), 
    in pwd varchar(100)
) 
as 
$$
declare
    c_count integer; 
begin
    SELECT COUNT(*) INTO c_count FROM "UserPwdStore" t WHERE "t"."Email" = email;

    if c_count = 0 then

       INSERT INTO "UserPwdStore" ("Email", "Pwd") VALUES (email, pwd);

    end if;

end;
$$
language plpgsql;