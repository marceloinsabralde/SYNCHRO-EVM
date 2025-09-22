using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class PlannedDatesOnActivityHaveOptionalTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "planned_finish_has_time",
                table: "activities",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "planned_start_has_time",
                table: "activities",
                type: "boolean",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "planned_finish_has_time", table: "activities");

            migrationBuilder.DropColumn(name: "planned_start_has_time", table: "activities");
        }
    }
}
