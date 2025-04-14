CREATE OR REPLACE FUNCTION fn_get_apikey(
    in vkey varchar(100)
) 
RETURNS TABLE (key VARCHAR, expired TIMESTAMPTZ) 
AS 
$$
BEGIN
RETURN QUERY SELECT t.key, t.expired FROM apikeystore t WHERE t.key = vkey;
END
$$ LANGUAGE plpgsql;