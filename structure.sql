SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;
SET default_tablespace = '';
SET default_table_access_method = heap;
CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);
ALTER TABLE public."__EFMigrationsHistory" OWNER TO "PerformNextGen";
CREATE TABLE public.activities (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    control_account_id uuid NOT NULL,
    reference_code text NOT NULL,
    name text NOT NULL,
    actual_start date,
    actual_finish date,
    planned_start date,
    planned_finish date
);
ALTER TABLE public.activities OWNER TO "PerformNextGen";
CREATE TABLE public.companies (
    id uuid NOT NULL,
    name text NOT NULL,
    description text
);
ALTER TABLE public.companies OWNER TO "PerformNextGen";
CREATE TABLE public.control_accounts (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    task_id uuid NOT NULL,
    reference_code text NOT NULL,
    name text NOT NULL,
    actual_start date,
    actual_finish date,
    planned_start date,
    planned_finish date
);
ALTER TABLE public.control_accounts OWNER TO "PerformNextGen";
CREATE TABLE public.units_of_measure (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    name text NOT NULL,
    symbol text NOT NULL
);
ALTER TABLE public.units_of_measure OWNER TO "PerformNextGen";
ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);
ALTER TABLE ONLY public.activities
    ADD CONSTRAINT pk_activities PRIMARY KEY (id);
ALTER TABLE ONLY public.companies
    ADD CONSTRAINT pk_companies PRIMARY KEY (id);
ALTER TABLE ONLY public.control_accounts
    ADD CONSTRAINT pk_control_accounts PRIMARY KEY (id);
ALTER TABLE ONLY public.units_of_measure
    ADD CONSTRAINT pk_units_of_measure PRIMARY KEY (id);
CREATE INDEX ix_activities_control_account_id ON public.activities USING btree (control_account_id);
ALTER TABLE ONLY public.activities
    ADD CONSTRAINT fk_activities_control_accounts_control_account_id FOREIGN KEY (control_account_id) REFERENCES public.control_accounts(id) ON DELETE CASCADE;
