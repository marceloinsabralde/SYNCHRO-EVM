using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.EventSource.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexesOnEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("ix_events_account_id", "events", "account_id");
            migrationBuilder.CreateIndex("ix_events_event_type", "events", "event_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("ix_events_account_id", "events");
            migrationBuilder.DropIndex("ix_events_event_type", "events");
        }
    }
}
