using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.EventSource.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityIdAndEntityTypeToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "entity_id",
                table: "events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<string>(
                name: "entity_type",
                table: "events",
                type: "text",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_events_entity_id_entity_type",
                table: "events",
                columns: new[] { "entity_id", "entity_type" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_events_entity_type",
                table: "events",
                column: "entity_type"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_events_entity_id_entity_type", table: "events");

            migrationBuilder.DropIndex(name: "ix_events_entity_type", table: "events");

            migrationBuilder.DropColumn(name: "entity_id", table: "events");

            migrationBuilder.DropColumn(name: "entity_type", table: "events");
        }
    }
}
