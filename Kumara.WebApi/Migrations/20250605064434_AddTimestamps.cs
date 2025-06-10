using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Kumara.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "units_of_measure",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "units_of_measure",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "progress_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "progress_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "materials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "materials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "material_activity_allocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "material_activity_allocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "created_at",
                table: "activities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );

            migrationBuilder.AddColumn<Instant>(
                name: "updated_at",
                table: "activities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L)
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "created_at", table: "units_of_measure");

            migrationBuilder.DropColumn(name: "updated_at", table: "units_of_measure");

            migrationBuilder.DropColumn(name: "created_at", table: "progress_entries");

            migrationBuilder.DropColumn(name: "updated_at", table: "progress_entries");

            migrationBuilder.DropColumn(name: "created_at", table: "materials");

            migrationBuilder.DropColumn(name: "updated_at", table: "materials");

            migrationBuilder.DropColumn(name: "created_at", table: "material_activity_allocations");

            migrationBuilder.DropColumn(name: "updated_at", table: "material_activity_allocations");

            migrationBuilder.DropColumn(name: "created_at", table: "control_accounts");

            migrationBuilder.DropColumn(name: "updated_at", table: "control_accounts");

            migrationBuilder.DropColumn(name: "created_at", table: "companies");

            migrationBuilder.DropColumn(name: "updated_at", table: "companies");

            migrationBuilder.DropColumn(name: "created_at", table: "activities");

            migrationBuilder.DropColumn(name: "updated_at", table: "activities");
        }
    }
}
