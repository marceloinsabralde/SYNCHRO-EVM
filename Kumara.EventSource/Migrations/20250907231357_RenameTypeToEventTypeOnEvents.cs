using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.EventSource.Migrations
{
    /// <inheritdoc />
    public partial class RenameTypeToEventTypeOnEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "type", table: "events", newName: "event_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "event_type", table: "events", newName: "type");
        }
    }
}
