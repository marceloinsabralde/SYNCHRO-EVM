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
CREATE TABLE public.material_activity_allocations (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    material_id uuid NOT NULL,
    activity_id uuid NOT NULL,
    quantity_unit_of_measure_id uuid NOT NULL,
    quantity_at_complete numeric NOT NULL
);
ALTER TABLE public.material_activity_allocations OWNER TO "PerformNextGen";
CREATE TABLE public.materials (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    name text NOT NULL,
    resource_role_id uuid NOT NULL,
    quantity_unit_of_measure_id uuid NOT NULL
);
ALTER TABLE public.materials OWNER TO "PerformNextGen";
CREATE TABLE public.progress_entries (
    id uuid NOT NULL,
    itwin_id uuid NOT NULL,
    activity_id uuid NOT NULL,
    material_id uuid NOT NULL,
    quantity_unit_of_measure_id uuid NOT NULL,
    quantity_delta numeric NOT NULL,
    progress_date date NOT NULL
);
ALTER TABLE public.progress_entries OWNER TO "PerformNextGen";
CREATE VIEW public.progress_summaries AS
 SELECT itwin_id,
    activity_id,
    material_id,
    quantity_unit_of_measure_id,
    sum(quantity_delta) AS quantity_to_date,
    quantity_at_complete,
    (quantity_at_complete - sum(quantity_delta)) AS quantity_to_complete,
    json_agg(entry_summary) FILTER (WHERE (row_number <= 10)) AS recent_progress_entries
   FROM ( SELECT progress_entries.id,
            progress_entries.itwin_id,
            progress_entries.activity_id,
            progress_entries.material_id,
            progress_entries.quantity_unit_of_measure_id,
            progress_entries.quantity_delta,
            material_activity_allocations.quantity_at_complete,
            ( SELECT row_to_json(_.*) AS row_to_json
                   FROM ( SELECT progress_entries.id,
                            progress_entries.quantity_delta,
                            progress_entries.progress_date) _) AS entry_summary,
            row_number() OVER (PARTITION BY progress_entries.itwin_id, progress_entries.activity_id, progress_entries.material_id, progress_entries.quantity_unit_of_measure_id ORDER BY progress_entries.progress_date DESC) AS row_number
           FROM (public.progress_entries
             JOIN public.material_activity_allocations ON (((material_activity_allocations.itwin_id = progress_entries.itwin_id) AND (material_activity_allocations.activity_id = progress_entries.activity_id) AND (material_activity_allocations.material_id = progress_entries.material_id) AND (material_activity_allocations.quantity_unit_of_measure_id = progress_entries.quantity_unit_of_measure_id))))
          ORDER BY progress_entries.progress_date DESC) unnamed_subquery
  GROUP BY itwin_id, activity_id, material_id, quantity_unit_of_measure_id, quantity_at_complete;
ALTER VIEW public.progress_summaries OWNER TO "PerformNextGen";
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
ALTER TABLE ONLY public.material_activity_allocations
    ADD CONSTRAINT pk_material_activity_allocations PRIMARY KEY (id);
ALTER TABLE ONLY public.materials
    ADD CONSTRAINT pk_materials PRIMARY KEY (id);
ALTER TABLE ONLY public.progress_entries
    ADD CONSTRAINT pk_progress_entries PRIMARY KEY (id);
ALTER TABLE ONLY public.units_of_measure
    ADD CONSTRAINT pk_units_of_measure PRIMARY KEY (id);
CREATE INDEX ix_activities_control_account_id ON public.activities USING btree (control_account_id);
CREATE INDEX ix_material_activity_allocations_activity_id ON public.material_activity_allocations USING btree (activity_id);
CREATE INDEX ix_material_activity_allocations_material_id ON public.material_activity_allocations USING btree (material_id);
CREATE INDEX ix_material_activity_allocations_quantity_unit_of_measure_id ON public.material_activity_allocations USING btree (quantity_unit_of_measure_id);
CREATE INDEX ix_materials_quantity_unit_of_measure_id ON public.materials USING btree (quantity_unit_of_measure_id);
CREATE INDEX ix_progress_entries_activity_id ON public.progress_entries USING btree (activity_id);
CREATE INDEX ix_progress_entries_material_id ON public.progress_entries USING btree (material_id);
CREATE INDEX ix_progress_entries_quantity_unit_of_measure_id ON public.progress_entries USING btree (quantity_unit_of_measure_id);
ALTER TABLE ONLY public.activities
    ADD CONSTRAINT fk_activities_control_accounts_control_account_id FOREIGN KEY (control_account_id) REFERENCES public.control_accounts(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.material_activity_allocations
    ADD CONSTRAINT fk_material_activity_allocations_activities_activity_id FOREIGN KEY (activity_id) REFERENCES public.activities(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.material_activity_allocations
    ADD CONSTRAINT fk_material_activity_allocations_materials_material_id FOREIGN KEY (material_id) REFERENCES public.materials(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.material_activity_allocations
    ADD CONSTRAINT fk_material_activity_allocations_units_of_measure_quantity_uni FOREIGN KEY (quantity_unit_of_measure_id) REFERENCES public.units_of_measure(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.materials
    ADD CONSTRAINT fk_materials_units_of_measure_quantity_unit_of_measure_id FOREIGN KEY (quantity_unit_of_measure_id) REFERENCES public.units_of_measure(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.progress_entries
    ADD CONSTRAINT fk_progress_entries_activities_activity_id FOREIGN KEY (activity_id) REFERENCES public.activities(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.progress_entries
    ADD CONSTRAINT fk_progress_entries_materials_material_id FOREIGN KEY (material_id) REFERENCES public.materials(id) ON DELETE CASCADE;
ALTER TABLE ONLY public.progress_entries
    ADD CONSTRAINT fk_progress_entries_units_of_measure_quantity_unit_of_measure_ FOREIGN KEY (quantity_unit_of_measure_id) REFERENCES public.units_of_measure(id) ON DELETE CASCADE;
