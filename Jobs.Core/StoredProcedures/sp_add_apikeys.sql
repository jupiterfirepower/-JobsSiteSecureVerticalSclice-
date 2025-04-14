create or replace procedure sp_add_apikeys(
    in key varchar(100), 
    in expired TIMESTAMPTZ)
as 
$$
begin
    INSERT INTO apikeystore (key, expired) VALUES (key, expired);
end;
$$
language plpgsql;