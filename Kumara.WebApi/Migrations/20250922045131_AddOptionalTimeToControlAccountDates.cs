using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalTimeToControlAccountDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "planned_start",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(LocalDate),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Instant>(
                name: "planned_finish",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(LocalDate),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Instant>(
                name: "actual_start",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(LocalDate),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Instant>(
                name: "actual_finish",
                table: "control_accounts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(LocalDate),
                oldType: "date",
                oldNullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "actual_finish_has_time",
                table: "control_accounts",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "actual_start_has_time",
                table: "control_accounts",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "planned_finish_has_time",
                table: "control_accounts",
                type: "boolean",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "planned_start_has_time",
                table: "control_accounts",
                type: "boolean",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "actual_finish_has_time", table: "control_accounts");

            migrationBuilder.DropColumn(name: "actual_start_has_time", table: "control_accounts");

            migrationBuilder.DropColumn(name: "planned_finish_has_time", table: "control_accounts");

            migrationBuilder.DropColumn(name: "planned_start_has_time", table: "control_accounts");

            migrationBuilder.AlterColumn<LocalDate>(
                name: "planned_start",
                table: "control_accounts",
                type: "date",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<LocalDate>(
                name: "planned_finish",
                table: "control_accounts",
                type: "date",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<LocalDate>(
                name: "actual_start",
                table: "control_accounts",
                type: "date",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<LocalDate>(
                name: "actual_finish",
                table: "control_accounts",
                type: "date",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true
            );
        }
    }
}
