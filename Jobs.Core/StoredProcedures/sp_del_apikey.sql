create or replace procedure sp_del_apikey(
    in vkey varchar(100)
)
as 
$$
begin
DELETE FROM apikeystore WHERE key = vkey;
end;
$$
language plpgsql;