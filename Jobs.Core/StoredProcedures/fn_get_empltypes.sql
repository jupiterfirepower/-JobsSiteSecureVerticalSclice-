CREATE OR REPLACE FUNCTION fn_get_empltypes() 
RETURNS TABLE ("EmploymentTypeId" INTEGER, "EmploymentTypeName" VARCHAR, "Created" TIMESTAMPTZ, "Modified" TIMESTAMPTZ) 
AS 
$$
BEGIN
RETURN QUERY SELECT "t"."EmploymentTypeId", "t"."EmploymentTypeName", "t"."Created", "t"."Modified" FROM "EmploymentTypes" t;
END
$$ LANGUAGE plpgsql;