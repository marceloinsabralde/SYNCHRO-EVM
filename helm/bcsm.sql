-- Copyright (c) Bentley Systems, Incorporated. All rights reserved.
DO $$
  DECLARE
    dbname text := current_database();
    dbuser text := 'deploy';
    dbschema text := 'public';
  BEGIN
    EXECUTE format('GRANT CREATE ON DATABASE %I TO %I;', dbname, dbuser);
    EXECUTE format('GRANT CREATE ON SCHEMA %I TO %I;', dbschema, dbuser);
  END
$$;
