using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Kumara.EventSourceToo.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    itwin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    triggered_by_user_subject = table.Column<Guid>(type: "uuid", nullable: true),
                    triggered_by_user_at = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_at = table.Column<Instant>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => new { x.id, x.account_id });
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_events_itwin_id",
                table: "events",
                column: "itwin_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "events");
        }
    }
}
