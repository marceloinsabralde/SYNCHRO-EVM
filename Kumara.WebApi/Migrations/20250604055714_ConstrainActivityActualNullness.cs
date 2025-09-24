using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ConstrainActivityActualNullness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                ALTER TABLE activities
                ADD CONSTRAINT ck_activities_actual_start_has_time_nullness
                CHECK ((actual_start IS NOT NULL) = (actual_start_has_time IS NOT NULL)),
                ADD CONSTRAINT ck_activities_actual_finish_has_time_nullness
                CHECK ((actual_finish IS NOT NULL) = (actual_finish_has_time IS NOT NULL));
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                ALTER TABLE activities
                DROP CONSTRAINT ck_activities_actual_start_has_time_nullness,
                DROP CONSTRAINT ck_activities_actual_finish_has_time_nullness;
                "
            );
        }
    }
}
