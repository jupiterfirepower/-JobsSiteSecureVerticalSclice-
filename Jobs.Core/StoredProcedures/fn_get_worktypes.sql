CREATE OR REPLACE FUNCTION fn_get_worktypes() 
RETURNS TABLE ("WorkTypeId" INTEGER, "WorkTypeName" VARCHAR, "Created" TIMESTAMPTZ, "Modified" TIMESTAMPTZ) 
AS 
$$
BEGIN
RETURN QUERY SELECT "t"."WorkTypeId", "t"."WorkTypeName", "t"."Created", "t"."Modified" FROM "WorkTypes" t;
END
$$ LANGUAGE plpgsql;