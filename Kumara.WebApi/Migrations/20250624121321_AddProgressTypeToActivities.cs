using Kumara.WebApi.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressTypeToActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "progress_type",
                table: "activities",
                type: "text",
                nullable: false,
                defaultValue: ActivityProgressType.Manual.ToString()
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "progress_type", table: "activities");
        }
    }
}
