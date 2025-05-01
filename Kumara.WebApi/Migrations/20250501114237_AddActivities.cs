using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.Migrations
{
    /// <inheritdoc />
    public partial class AddActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itwin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    control_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    actual_start = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_finish = table.Column<DateOnly>(type: "date", nullable: true),
                    planned_start = table.Column<DateOnly>(type: "date", nullable: true),
                    planned_finish = table.Column<DateOnly>(type: "date", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_activities_control_accounts_control_account_id",
                        column: x => x.control_account_id,
                        principalTable: "control_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_activities_control_account_id",
                table: "activities",
                column: "control_account_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "activities");
        }
    }
}
