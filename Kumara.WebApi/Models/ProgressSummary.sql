-- Copyright (c) Bentley Systems, Incorporated. All rights reserved.
SELECT
  itwin_id,
  activity_id,
  material_id,
  quantity_unit_of_measure_id,
  SUM(quantity_delta) AS quantity_to_date,
  quantity_at_complete,
  quantity_at_complete - SUM(quantity_delta) AS quantity_to_complete,
  json_agg(entry_summary ORDER BY row_number) FILTER (WHERE row_number <= 10) AS recent_progress_entries
FROM (
  SELECT
    progress_entries.itwin_id,
    progress_entries.activity_id,
    progress_entries.material_id,
    progress_entries.quantity_unit_of_measure_id,
    progress_entries.quantity_delta,
    material_activity_allocations.quantity_at_complete,
    progress_entries.created_at,
    progress_entries.updated_at,
    (SELECT row_to_json(_) FROM (
        SELECT
            progress_entries.id,
            progress_entries.quantity_delta,
            progress_entries.progress_date,
            progress_entries.created_at,
            progress_entries.updated_at
        )
    _) AS entry_summary,
    row_number() OVER (PARTITION BY progress_entries.itwin_id, progress_entries.activity_id, progress_entries.material_id, progress_entries.quantity_unit_of_measure_id ORDER BY progress_entries.progress_date DESC, progress_entries.id DESC) as row_number
  FROM
    progress_entries
  JOIN material_activity_allocations using (
    itwin_id,
    activity_id,
    material_id,
    quantity_unit_of_measure_id
  )
)
GROUP BY itwin_id, activity_id, material_id, quantity_unit_of_measure_id, quantity_at_complete
