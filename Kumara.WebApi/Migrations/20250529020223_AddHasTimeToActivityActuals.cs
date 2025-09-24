using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHasTimeToActivityActuals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "actual_start_has_time",
                table: "activities",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "actual_finish_has_time",
                table: "activities",
                type: "boolean",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "actual_start_has_time", table: "activities");

            migrationBuilder.DropColumn(name: "actual_finish_has_time", table: "activities");
        }
    }
}
