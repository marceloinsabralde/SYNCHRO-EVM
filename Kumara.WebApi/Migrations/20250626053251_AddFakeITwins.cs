using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kumara.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFakeITwins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fake_itwins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    @class = table.Column<string>(name: "class", type: "text", nullable: true),
                    sub_class = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    number = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    geographic_location = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    time_zone = table.Column<string>(type: "text", nullable: true),
                    iana_time_zone = table.Column<string>(type: "text", nullable: true),
                    data_center_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_center_location = table.Column<string>(type: "text", nullable: true),
                    billing_country = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    solution_type = table.Column<string>(type: "text", nullable: true),
                    itwin_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_owner_type = table.Column<int>(type: "integer", nullable: true),
                    auth_mode = table.Column<int>(type: "integer", nullable: true),
                    is_template = table.Column<bool>(type: "boolean", nullable: true),
                    allow_external_users = table.Column<bool>(type: "boolean", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    created_date_time = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_date_time = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fake_itwins", x => x.id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "fake_itwins");
        }
    }
}
