create or replace procedure sp_save_vac_emptypes(
    in vacancyId int, 
    in empTypes varchar(15)
) 
as 
$$
declare
    v_count integer; 
    et_count integer; 
begin
    SELECT COUNT(*) INTO v_count FROM "Vacancies" t WHERE "t"."VacancyId" = vacancyId;

    WITH t AS (
  	  SELECT DISTINCT unnest (string_to_array(empTypes, ',')::integer[]) as emptypeid
       )
       SELECT count(t.emptypeid) INTO et_count
       FROM t 
         WHERE NOT EXISTS 
           (SELECT * 
              FROM "EmploymentTypes" e
              WHERE "e"."EmploymentTypeId" = t.emptypeid);

    if v_count > 0 and et_count = 0 then

       DELETE FROM "VacancyEmploymentTypes" WHERE "VacancyId" = vacancyId;

       WITH t AS (
  	   SELECT DISTINCT unnest (string_to_array(empTypes, ',')::integer[]) as emptypeid
       ), r AS (
          SELECT t.emptypeid FROM t INNER JOIN "EmploymentTypes" e
          ON "e"."EmploymentTypeId" = t.emptypeid
       )
       INSERT INTO "VacancyEmploymentTypes" 
       SELECT vacancyId as VacancyId, r.emptypeid as EmploymentTypeID FROM r;

    end if;

end;
$$
language plpgsql;