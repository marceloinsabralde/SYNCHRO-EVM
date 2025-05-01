using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "progress_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itwin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_unit_of_measure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_delta = table.Column<decimal>(type: "numeric", nullable: false),
                    progress_date = table.Column<DateOnly>(type: "date", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_progress_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_progress_entries_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_progress_entries_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_progress_entries_units_of_measure_quantity_unit_of_measure_",
                        column: x => x.quantity_unit_of_measure_id,
                        principalTable: "units_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_progress_entries_activity_id",
                table: "progress_entries",
                column: "activity_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_progress_entries_material_id",
                table: "progress_entries",
                column: "material_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_progress_entries_quantity_unit_of_measure_id",
                table: "progress_entries",
                column: "quantity_unit_of_measure_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "progress_entries");
        }
    }
}
