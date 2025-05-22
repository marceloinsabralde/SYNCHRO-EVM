using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialActivityAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "material_activity_allocations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itwin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_unit_of_measure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity_at_complete = table.Column<decimal>(type: "numeric", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material_activity_allocations", x => x.id);
                    table.ForeignKey(
                        name: "fk_material_activity_allocations_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_material_activity_allocations_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_material_activity_allocations_units_of_measure_quantity_uni",
                        column: x => x.quantity_unit_of_measure_id,
                        principalTable: "units_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_material_activity_allocations_activity_id",
                table: "material_activity_allocations",
                column: "activity_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_material_activity_allocations_material_id",
                table: "material_activity_allocations",
                column: "material_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_material_activity_allocations_quantity_unit_of_measure_id",
                table: "material_activity_allocations",
                column: "quantity_unit_of_measure_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "material_activity_allocations");
        }
    }
}
