using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.Migrations
{
    /// <inheritdoc />
    public partial class AddItwinIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_units_of_measure_itwin_id",
                table: "units_of_measure",
                column: "itwin_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_progress_entries_itwin_id",
                table: "progress_entries",
                column: "itwin_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_materials_itwin_id",
                table: "materials",
                column: "itwin_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_material_activity_allocations_itwin_id",
                table: "material_activity_allocations",
                column: "itwin_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_control_accounts_itwin_id",
                table: "control_accounts",
                column: "itwin_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_activities_itwin_id",
                table: "activities",
                column: "itwin_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_units_of_measure_itwin_id",
                table: "units_of_measure"
            );

            migrationBuilder.DropIndex(
                name: "ix_progress_entries_itwin_id",
                table: "progress_entries"
            );

            migrationBuilder.DropIndex(name: "ix_materials_itwin_id", table: "materials");

            migrationBuilder.DropIndex(
                name: "ix_material_activity_allocations_itwin_id",
                table: "material_activity_allocations"
            );

            migrationBuilder.DropIndex(
                name: "ix_control_accounts_itwin_id",
                table: "control_accounts"
            );

            migrationBuilder.DropIndex(name: "ix_activities_itwin_id", table: "activities");
        }
    }
}
