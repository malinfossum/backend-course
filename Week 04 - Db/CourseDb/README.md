# CourseDb

Week 4: relations: primary and foreign keys, many-to-many, `INNER JOIN`, `LEFT JOIN`.

No application this week. It is a course administration database and five SQL scripts that build it and
then ask it questions. Students and courses are many-to-many, so the relationship gets a table:
`Enrollments`.

- `Enrollments` is not just two foreign keys. `EnrolledUtc` and `Status` describe *this student on
  this course*. Neither value has anywhere else to live, which is what makes the join table a real
  table rather than plumbing.
- `INNER JOIN` drops the student who is enrolled in nothing; `LEFT JOIN` keeps him with `NULL` on the
  other side. Adding `Courses` has to be a `LEFT JOIN` too, or he is thrown out again on the second
  join.
- Counting students per course needs `COUNT(e.Id)`, not `COUNT(*)`. The left join hands `COUNT(*)` one
  row of `NULL`s for the empty course, so it reports 1 where the answer is 0.
- Duplicate enrollments are stopped by `UNIQUE (StudentId, CourseId)`. Neither column is unique alone
  (both are supposed to repeat). It is the pair that may occur only once.
- `547` is not one error. Foreign key on insert, foreign key on delete and `CHECK` all report it;
  only `UNIQUE` gets its own number, `2627`. The constraint name in the message is what tells them
  apart, which is the argument for naming them.

## Running it

Needs a local SQL Server. The scripts run in order, and `01` drops and reseeds, so `04` and `05` have
to be run again after it. From the repository root:

```bash
sqlcmd -S localhost -E -C -b -i "Week 04 - Db/CourseDb/Database/01-create-database.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/02-joins.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/03-constraints.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/04-status.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/05-challenges.sql"
```

`03` is meant to fail: three statements the foreign keys refuse, caught and printed so the whole
script still runs.
