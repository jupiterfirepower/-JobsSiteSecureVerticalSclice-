create or replace procedure sp_save_vac_worktypes(
    in vacancyId int, 
    in workTypes varchar(10)
) 
as 
$$
declare
    v_count integer; 
    wt_count integer; 
begin
    SELECT COUNT(*) INTO v_count FROM "Vacancies" t WHERE "t"."VacancyId" = vacancyId;

    WITH t AS (
  	  SELECT DISTINCT unnest (string_to_array(workTypes,  ',')::integer[]) as worktypeid
       )
       SELECT count(t.worktypeid) INTO wt_count
       FROM t 
         WHERE NOT EXISTS 
           (SELECT * 
              FROM "WorkTypes" w
              WHERE "w"."WorkTypeId" = t.worktypeid);

    if v_count > 0 and wt_count = 0 then

       DELETE FROM "VacancyWorkTypes" WHERE "VacancyId" = vacancyId;

       WITH t AS (
  	  SELECT DISTINCT unnest (string_to_array(workTypes,  ',')::integer[]) as worktypeid
       ), r AS (
          SELECT t.worktypeid FROM t INNER JOIN "WorkTypes" w
          ON "w"."WorkTypeId" = t.worktypeid
       )
       INSERT INTO "VacancyWorkTypes" 
       SELECT vacancyId as VacancyId, r.worktypeid as WorkTypeID FROM r;

    end if;

end;
$$
language plpgsql;